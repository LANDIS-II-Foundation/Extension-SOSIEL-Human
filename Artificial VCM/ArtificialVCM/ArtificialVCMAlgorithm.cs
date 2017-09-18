using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtificialVCM.Configuration;
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

            _outputFolder = @"Output\ArtificialVCM";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }

        public string Run()
        {
            var sites = new Site[] {Site.DefaultSite};

            Enumerable.Range(1, _configuration.AlgorithmConfiguration.NumberOfIterations).ForEach(iteration =>
            {
                Console.WriteLine("Starting {0} iteration", iteration);

                RunSosiel(sites);
            });

            return _outputFolder;
        }

        /// <inheritdoc />
        protected override List<IAgent> InitializeAgents()
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

            return agents;
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
                agent.GoalStates.ForEach(kvp =>
                {
                    agentState.GoalsState[kvp.Key] = kvp.Value;
                });

                //selects heuristics for first iteration
                KnowledgeHeuristicsHistory history = CreateHistory((ArtificialVCMAgent)agent);
                agentState.AddHeuristicHistory(history);

                states.Add(agent, agentState);
            });

            return states;
        }

        /// <summary>
        /// Creates an activated/matched heuristics history
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        private static KnowledgeHeuristicsHistory CreateHistory(ArtificialVCMAgent agent)
        {
            KnowledgeHeuristicsHistory history = new KnowledgeHeuristicsHistory();

            KnowledgeHeuristic[] firstIterationActivated
                = agent.AssignedKnowledgeHeuristics
                    .Where(kh => agent.AgentStateConfiguration.ActivatedKhOnFirstIteration.Contains(kh.Id))
                    .ToArray();

            history.Matched.AddRange(firstIterationActivated);
            history.Activated.AddRange(firstIterationActivated);

            return history;
        }
    }
}
