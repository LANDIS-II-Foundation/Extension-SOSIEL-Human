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
            agentList = AgentList.Generate(configuration);

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

                    //todo check
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
        /// Initialization of algorithm
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
        public void RunIteration(IEnumerable<ActiveSite> activeSites)
        {

        }
    }
}
