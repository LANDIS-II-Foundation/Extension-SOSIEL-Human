using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using Common.Configuration;
using Common.Algorithm;
using Common.Entities;
using Common.Helpers;
using Common.Randoms;
using Common.Models;
using Common.Processes;

namespace CL2_M7
{

    public sealed class CL2M7Algorithm : SosielAlgorithm<CL2M7Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 2 Model 7"; } }

        string _outputFolder;

        Configuration<CL2M7Agent> _configuration;


        //statistics
        List<AgentNumericValuesOutput> _agentContributions;



        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                RuleSelectionEnabled = true,
                AgentRandomizationEnabled = false
            };
        }

        public CL2M7Algorithm(Configuration<CL2M7Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _agentContributions = new List<AgentNumericValuesOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL2_M7";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }

        public string Run()
        {
            ExecuteAlgorithm();

            return _outputFolder;
        }

        protected override void InitializeAgents()
        {
            _numberOfAgents = _configuration.InitialState.AgentsState.Sum(astate => astate.NumberOfAgents);

            _agentList = AgentList.Generate(_numberOfAgents, _configuration.AgentConfiguration, _configuration.InitialState);
        }

        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            return IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents);
        }

        protected override void AfterInitialization()
        {

        }

        protected override void AfterAlgorithmExecuted()
        {
            StatisticHelper.Save(_agentContributions, $@"{_outputFolder}\agent_contributions_statistic.csv");
        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            _agentList.Agents.First().SetToCommon(Agent.VariablesUsedInCode.Iteration, iteration);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = _agentList.Agents.First();

            agent.SetToCommon(Agent.VariablesUsedInCode.CommonPoolC, _agentList.CalculateCommonC());
            agent.SetToCommon(Agent.VariablesUsedInCode.CommonPoolSize, _agentList.Agents);

            orderedAgents.AsParallel().ForAll(a =>
            {
                a[Agent.VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);
            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;

            _agentContributions.Add(StatisticHelper.CreateAgentValuesRecord(activeAgents, iteration, Agent.VariablesUsedInCode.AgentC));
        }


        private double CalculateAgentWellbeing(IAgent agent)
        {
            return agent[Agent.VariablesUsedInCode.AgentE] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * agent[Agent.VariablesUsedInCode.CommonPoolC] / (double)agent[Agent.VariablesUsedInCode.CommonPoolSize];
        }

        private double CalculatePoolWellbeing(IAgent agent)
        {
            return agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * agent[Agent.VariablesUsedInCode.CommonPoolC] / (double)agent[Agent.VariablesUsedInCode.CommonPoolSize];
        }
    }
}
