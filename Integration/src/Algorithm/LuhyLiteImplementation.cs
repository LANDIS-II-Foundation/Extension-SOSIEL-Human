using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;

namespace Landis.Extension.SOSIELHuman.Algorithm
{
    using Configuration;
    using Entities;
    using Helpers;
    using Randoms;
    using Output;


    public class LuhyLiteImplementation : SosielAlgorithm, IAlgorithm
    {
        public string Name { get { return "LuhyLiteImplementation"; } }

        private ConfigurationModel configuration;

        private ActiveSite[] activeSites;

        private Dictionary<ActiveSite, double> biomass;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary> 
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="activeSites">Enumerable of active sites from Landis</param>
        /// <param name="biomass">Active site biomass values which are updated each iteration</param>
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

        /// <summary>
        /// Executes agent initializing. It's the first initializing step. 
        /// </summary>
        protected override void InitializeAgents()
        {
            agentList = new AgentList();
            agentList.Initialize(configuration);

            numberOfAgentsAfterInitialize = agentList.Agents.Count;
        }

        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
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
                    agent.InitialStateConfiguration.GoalsState.ForEach(gs =>
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
        /// Creates an activated/matched rules history
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
        /// Executes algorithm initialization
        /// </summary>
        public void Initialize()
        {
            InitializeAgents();

            InitializeFirstIterationState();

            AfterInitialization();
        }



        /// <summary>
        /// Runs as many internal iterations as passed to the constructor
        /// </summary>
        public void RunIteration()
        {
            RunSosiel(activeSites);
        }

        /// <summary>
        /// Executes last preparations before runs the algorithm. Executes after InitializeAgents and InitializeFirstIterationState.
        /// </summary>
        protected override void AfterInitialization()
        {
            //call default implementation
            base.AfterInitialization();


            //----
            //set default values which were not defined in configuration file
            var feAgents = agentList.GetAgentsWithPrefix("FE");

            feAgents.ForEach(agent =>
            {
                agent[VariablesUsedInCode.Profit] = 0d;
            });



            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.ForEach(agent =>
            {

                agent[VariablesUsedInCode.Income] = 0d;
                agent[VariablesUsedInCode.Expenses] = 0d;
                agent[VariablesUsedInCode.Savings] = 0d;

                agent[VariablesUsedInCode.HouseholdSavings] = 0d;
            });

        }


        /// <summary>
        /// Executed before any cognitive process is started.
        /// </summary>
        /// <param name="iteration"></param>
        protected override void PreIterationCalculations(int iteration)
        {
            //call default implementation
            base.PreIterationCalculations(iteration);

            //----
            //calculate tourism value
            double averageBiomass = biomass.Values.Average();

            var fePrototypes = agentList.GetPrototypesWithPrefix("FE");

            fePrototypes.ForEach(hmProt =>
            {
                hmProt[VariablesUsedInCode.AverageBiomass] = averageBiomass;
            });

            var hmPrototypes = agentList.GetPrototypesWithPrefix("HM");

            hmPrototypes.ForEach(hmProt =>
            {
                hmProt[VariablesUsedInCode.Tourism] = averageBiomass >= hmProt[VariablesUsedInCode.TourismThreshold];
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
                    //accumulate savings
                    agent[VariablesUsedInCode.HouseholdSavings] += householdSavings;
                });
            });

        }

        /// <summary>
        /// Executed before action selection process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected override void BeforeActionSelection(IAgent agent, ActiveSite site)
        {
            //call default implementation
            base.BeforeActionSelection(agent, site);

            //if agent is FE, set to local variables current site biomass
            if (agent[VariablesUsedInCode.AgentType] == "Type1")
            {
                //set value of current site biomass to agent variable. 
                agent[VariablesUsedInCode.Biomass] = biomass[site];


                //drop total profit value 
                agent[VariablesUsedInCode.Profit] = 0;
            }
        }

        /// <summary>
        /// Executed after action taking process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected override void AfterActionTaking(IAgent agent, ActiveSite site)
        {
            //call default implementation
            base.AfterActionTaking(agent, site);


            if (agent[VariablesUsedInCode.AgentType] == "Type1")
            {
                //compute profit
                double profit = biomass[site] * agent[VariablesUsedInCode.ReductionPercentage] / 100d;
                //add computed profit to total profit
                agent[VariablesUsedInCode.Profit] += profit;

                //reduce biomass
                biomass[site] -= profit;
            }
        }

        /// <summary>
        /// Executed after PostIterationCalculations. Here we can collect all output data.
        /// </summary>
        /// <param name="iteration"></param>
        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);


            //save statistics for each agent
            agentList.ActiveAgents.ForEach(agent =>
            {
                AgentState agentState = iterations.Last.Value[agent];

                if (agent[VariablesUsedInCode.AgentType] == "Type1")
                {
                    double averageReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
                        .Where(ta => ta.VariableName == VariablesUsedInCode.ReductionPercentage).Average(ta => (double)ta.Value);

                    double profit = agent[VariablesUsedInCode.Profit];

                    double averageBiomass = agent[VariablesUsedInCode.AverageBiomass];

                    FEValuesOutput values = new FEValuesOutput()
                    {
                        Iteration = iteration,
                        AverageBiomass = averageBiomass,
                        AverageReductionPercentage = averageReductionPercentage,
                        BiomassReduction = profit
                    };


                    WriteToCSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_values.csv", agent.Id), values);
                }




                //all agent types 

                //save activation rule stat
                string[] activatedRules = agentState.RuleHistories.Values.SelectMany(rh => rh.Activated.Select(rule => rule.Id)).Distinct().ToArray();

                string[] notActivatedRules = agent.AssignedRules.Select(rule => rule.Id).Except(activatedRules).ToArray();

                HMRuleUsageOutput ruleUsage = new HMRuleUsageOutput()
                {
                    Iteration = iteration,
                    ActivatedRules = activatedRules,
                    NotActivatedRules = notActivatedRules
                };

                WriteToCSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_rules.csv", agent.Id), ruleUsage);
            });
        }
    }
}
