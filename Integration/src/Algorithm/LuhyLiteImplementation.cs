using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Algorithm
{
    using Configuration;
    using Entities;
    using Helpers;
    using Randoms;


    public class LuhyLiteImplementation : SosielAlgorithm, IAlgorithm
    {
        public string Name { get { return "LuhyLiteImplementation"; } }

        private ConfigurationModel configuration;

        private ActiveSite[] activeSites;

        private Dictionary<ActiveSite, double> biomass;

        public LuhyLiteImplementation(int numberOfIterations,
            ConfigurationModel configuration,
            IEnumerable<ActiveSite> activeSites,
            Dictionary<ActiveSite, double> biomass)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel)
            )
        {
            this.configuration = configuration;

            this.activeSites = activeSites.ToArray();

            this.biomass = biomass;
        }

        protected override void InitializeAgents()
        {
            agentList = new AgentList();
            agentList.Initialize(configuration);

            numberOfAgentsAfterInitialize = agentList.Agents.Count;
        }

        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            Dictionary<IAgent, AgentState> temp = new Dictionary<IAgent, AgentState>();


            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState agentState = AgentState.Create(agent.Prototype.IsSiteOriented);


                //randomly generates goal importance
                if (configuration.InitialState.GenerateGoalImportance)
                {
                    double unadjustedProportion = 1;

                    var goals = agent.AssignedGoals.Join(agent.InitialStateConfiguration.AssignedGoals, g => g.Name, gs => gs, (g, gs) => new { g, gs }).ToArray();

                    int numberOfRankingGoals = goals.Count(o => o.g.RankingEnabled);

                    goals.OrderByDescending(o => o.g.RankingEnabled).ForEach((o, i) =>
                    {
                        double proportion = unadjustedProportion;

                        if (o.g.RankingEnabled)
                        {
                            if (numberOfRankingGoals > 1 && i < numberOfRankingGoals - 1)
                            {
                                double d;


                                if (agent.ContainsVariable(VariablesUsedInCode.Mean) && agent.ContainsVariable(VariablesUsedInCode.StdDev))
                                    d = NormalDistributionRandom.GetInstance.Next(agent[VariablesUsedInCode.Mean], agent[VariablesUsedInCode.StdDev]);
                                else
                                    d = NormalDistributionRandom.GetInstance.Next();

                                if (d < 0)
                                    d = 0;

                                if (d > 1)
                                    d = 1;

                                proportion = Math.Round(d, 1, MidpointRounding.AwayFromZero);
                            }

                            unadjustedProportion = Math.Round(unadjustedProportion - proportion, 1, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            proportion = 0;
                        }

                        GoalState goalState = new GoalState(0, o.g.FocalValue, proportion);

                        agentState.GoalsState.Add(o.g, goalState);
                    });
                }
                else
                {
                    agent.InitialStateConfiguration.GoalState.ForEach(gs =>
                    {
                        Goal goal = agent.AssignedGoals.First(g => g.Name == gs.Key);

                        GoalState goalState = new GoalState(0, goal.FocalValue, gs.Value.Importance);

                        agentState.GoalsState.Add(goal, goalState);
                    });
                }

                //selects rules for first iteration
                if (agent.Prototype.IsSiteOriented)
                {
                    RuleHistory history = CreateRuleHistory(configuration.InitialState, agent);

                    activeSites.ForEach(activeSite =>
                    {
                        agentState.AddRuleHistory(history, activeSite);

                        if (configuration.InitialState.RandomlySelectRule)
                        {
                            history = CreateRuleHistory(configuration.InitialState, agent);
                        }
                    });
                }
                else
                {
                    agentState.AddRuleHistory(CreateRuleHistory(configuration.InitialState, agent));
                }

                temp.Add(agent, agentState);
            });

            return temp;
        }

        /// <summary>
        /// Creates one rule history
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        private static RuleHistory CreateRuleHistory(InitialStateConfiguration configuration, IAgent agent)
        {
            RuleHistory history = new RuleHistory();

            if (configuration.RandomlySelectRule)
            {
                agent.AssignedRules.Where(r => r.IsAction && r.IsCollectiveAction == false).GroupBy(r => new { r.RuleSet, r.RuleLayer })
                    .ForEach(g =>
                    {
                        Rule selectedRule = g.RandomizeOne();

                        history.Matched.Add(selectedRule);
                        history.Activated.Add(selectedRule);
                    });
            }
            else
            {
                Rule[] firstIterationsRule = agent.InitialStateConfiguration.ActivatedRulesOnFirstIteration.Select(rId => agent.AssignedRules.First(ar => ar.Id == rId)).ToArray();

                history.Matched.AddRange(firstIterationsRule);
                history.Activated.AddRange(firstIterationsRule);
            }

            return history;
        }



        /// <summary>
        /// Initialization of the algorithm
        /// </summary>
        public void Initialize()
        {
            InitializeAgents();

            InitializeFirstIterationState();

            AfterInitialization();
        }



        /// <summary>
        /// Runs as many iterations as passed to the constructor
        /// </summary>
        public void RunIteration()
        {
            RunSosiel(activeSites);
        }


        protected override void AfterInitialization()
        {
            //call default implementation
            base.AfterInitialization();

            //----
            //set default values which were not defined in configuration file
            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.ForEach(agent =>
            {

                agent[VariablesUsedInCode.Income] = 0;
                agent[VariablesUsedInCode.Expenses] = 0;
                agent[VariablesUsedInCode.Savings] = 0;

            });

        }

        protected override void BeforeActionSelection(IAgent agent, ActiveSite site)
        {
            //call default implementation
            base.BeforeActionSelection(agent, site);

            //if agent is FE, set to local variables current site biomass
            if (agent[VariablesUsedInCode.AgentType] == "Type1")
            {
                agent[VariablesUsedInCode.Biomass] = biomass[site];
            }
        }


        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            //call default implementation
            base.PreIterationCalculations(iteration, orderedAgents);

            //----
            //calculate tourism value
            var hmPrototypes = agentList.GetPrototypesWithPrefix("HM");

            double totalBiomass = biomass.Values.Sum();

            hmPrototypes.ForEach(hmProt =>
            {
                hmProt[VariablesUsedInCode.Tourism] = totalBiomass >= hmProt[VariablesUsedInCode.TourismThreshold];
            });


            //----
            //calculate household values (income, expenses, savings) for each agent in specific household
            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.GroupBy(agent => agent[VariablesUsedInCode.Household]).ForEach(householdAgents =>
            {
                double householdIncome = householdAgents.Sum(agent => (double)agent[VariablesUsedInCode.Income]);
                double householdExpenses = householdAgents.Sum(agent => (double)agent[VariablesUsedInCode.Expenses]);
                double householdSavings = householdIncome - householdExpenses;

                householdAgents.ForEach(agent =>
                {
                    agent[VariablesUsedInCode.HouseholdIncome] = householdIncome;
                    agent[VariablesUsedInCode.HouseholdExpenses] = householdExpenses;
                    agent[VariablesUsedInCode.HouseholdSavings] = householdSavings;
                });
            });

        }


        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            //call default implementation
            base.PreIterationCalculations(iteration, orderedAgents);

            //----
            //calculate tourism value
            var hmPrototypes = agentList.GetPrototypesWithPrefix("FE");

            double totalBiomass = biomass.Values.Sum();

            hmPrototypes.ForEach(hmProt =>
            {
                hmProt[VariablesUsedInCode.Tourism] = totalBiomass >= hmProt[VariablesUsedInCode.TourismThreshold];
            });
        }

    }
}
