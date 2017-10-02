﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtificialVCM.Configuration;
using ArtificialVCM.Output;
using Common.Algorithm;
using Common.Configuration;
using Common.Entities;
using Common.Exceptions;
using Common.Helpers;

namespace ArtificialVCM
{
    public sealed class ArtificialVCMAlgorithm : SosielAlgorithm, IAlgorithm
    {
        public string Name { get { return "Cognitive level 4 Artificial VCM"; } }

        string _outputFolder;

        ConfigurationModel _configuration;

        public static ProcessesConfiguration GetProcessConfiguration()
        {
            return new ProcessesConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                HeuristicSelectionEnabled = true,
                HeuristicSelectionPart2Enabled = false,
                SocialLearningEnabled = false,
                CounterfactualThinkingEnabled = true,
                InnovationEnabled = true,
                ReproductionEnabled = false,
                AgentRandomizationEnabled = true,
                AgentsDeactivationEnabled = false,
                AlgorithmStopIfAllAgentsSelectDoNothing = false
            };
        }

        public ArtificialVCMAlgorithm(ConfigurationModel configuration) : base(1, GetProcessConfiguration())
        {
            _configuration = configuration;

            _outputFolder = @"Output\";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }

        public string Run()
        {
            Initialize();

            var sites = new Site[] {Site.DefaultSite};

            Enumerable.Range(1, _configuration.AlgorithmConfiguration.NumberOfIterations).ForEach(iteration =>
            {
                Console.WriteLine("Starting {0} iteration", iteration);

                RunSosiel(sites);
            });

            return _outputFolder;
        }

        /// <summary>
        /// Executes algorithm initialization
        /// </summary>
        public void Initialize()
        {
            InitializeAgents();

            AfterInitialization();
        }

        /// <inheritdoc />
        protected override void InitializeAgents()
        {
            var agents = new List<IAgent>();

            Dictionary<string, AgentPrototype> agentPrototypes = _configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = _configuration.InitialState;

            //add donothing heuristic if necessary
            agentPrototypes.ForEach(kvp =>
            {
                AgentPrototype prototype = kvp.Value;

                string prototypeName = kvp.Key;

                if (prototype.MentalProto.Any(set => set.Layers.Any(layer => layer.LayerConfiguration.UseDoNothing)))
                {
                    var added = prototype.AddDoNothingHeuristics();

                    initialState.AgentsState.Where(aState => aState.PrototypeOfAgent == prototypeName).ForEach(aState =>
                    {
                        aState.AssignedKnowledgeHeuristics = aState.AssignedKnowledgeHeuristics.Concat(added).ToArray();
                    });
                }
            });

            //calculate focal values (max possible difference)
            var agentsCount = initialState.AgentsState.Sum(state => state.NumberOfAgents);
            agentPrototypes.ForEach(kvp =>
            {
                var prototype = kvp.Value;

                prototype.Goals.ForEach(goal =>
                {
                    var e = prototype[AlgorithmVariables.E];
                    var m = prototype[AlgorithmVariables.M];

                    switch (goal.Name)
                    {
                        case "G1":
                            goal.FocalValue = e + m * e * (agentsCount - 1) / agentsCount;
                            break;
                        case "G2":
                            goal.FocalValue = m * e;
                            break;
                    }
                });
            });

            //create agents, groupby is used for saving agents numeration, e.g. FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentPrototype prototype = agentPrototypes[agentStateGroup.Key];

                int index = 1;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        Agent agent = ArtificialVCMAgent.CreateAgent(agentState, prototype);
                        agent.SetId(index);

                        agents.Add(agent);

                        index++;
                    }
                });
            });

            agentList =  new AgentList(agents, agentPrototypes.Select(kvp=>kvp.Value).ToList());
        }

        protected override void AfterInitialization()
        {
            base.AfterInitialization();

            CalculateCommonPoolProfit();
        }

        /// <inheritdoc />
        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            var states = new Dictionary<IAgent, AgentState>();

            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState agentState = AgentState.Create(agent.Prototype.IsSiteOriented);

                //copy generated goal importance
                agent.InitialGoalStates.ForEach(kvp =>
                {
                    agentState.GoalsState[kvp.Key] = kvp.Value;
                });

                states.Add(agent, agentState);
            });

            return states;
        }

        /// <inheritdoc />
        protected override void PostIterationCalculations(int iteration)
        {
            base.PostIterationCalculations(iteration);

            CalculateCommonPoolProfit();

            agentList.ActiveAgents.ForEach(agent =>
            {
                agent[AlgorithmVariables.AgentProfit] = agent[AlgorithmVariables.E] - agent[AlgorithmVariables.AgentC] +
                                                        agent[AlgorithmVariables.CommonProfit];
            });
        }

        /// <summary>
        /// Calculates the common pool profit.
        /// </summary>
        private void CalculateCommonPoolProfit()
        {
            var commonProfit = agentList.ActiveAgents
                .Select(agent => (double) agent[AlgorithmVariables.M] * (double) agent[AlgorithmVariables.AgentC])
                .Average();

            var firstAgent = agentList.Agents.First();
            firstAgent.SetToCommon(AlgorithmVariables.CommonProfit, commonProfit);
        }

        /// <inheritdoc />
        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            var firstAgent = agentList.Agents.First();

            WriteToCSVHelper.AppendTo(_outputFolder + AverageContribution.FileName, new AverageContribution
            {
                Iteration = iteration,
                CommonPoolProfit = firstAgent[AlgorithmVariables.CommonProfit]
            });

            agentList.Agents.ForEach(agent =>
            {
                var details = new AgentDetails
                {
                    Iteration = iteration,
                    AgentId = agent.Id,
                    AgentProfile = agent[AlgorithmVariables.AgentProfile],
                    G1Importance = GetG1Importance(agent),
                    G2Importance = GetG2Importance(agent),
                    AgentContribution = agent[AlgorithmVariables.AgentC],
                    NumberOfKH = agent.AssignedKnowledgeHeuristics.Count
                };

                WriteToCSVHelper.AppendTo(_outputFolder + string.Format(AgentDetails.FileName, agent.Id), details);
            });
        }

        /// <summary>
        /// Gets the first goal importance.
        /// </summary>
        private double GetG1Importance(IAgent agent)
        {
            return agent.ContainsVariable(AlgorithmVariables.G1Importance) ? agent[AlgorithmVariables.G1Importance] : 0;
        }

        /// <summary>
        /// Gets the second goal importance.
        /// </summary>
        private double GetG2Importance(IAgent agent)
        {
            return agent.ContainsVariable(AlgorithmVariables.G2Importance) ? agent[AlgorithmVariables.G2Importance] : 0;
        }
    }
}
