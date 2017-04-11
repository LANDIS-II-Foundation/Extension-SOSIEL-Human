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

namespace CL2_M9
{
    public sealed class CL2M9Algorithm : SosielAlgorithm<CL2M9Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 2 Model 9"; } }

        string _outputFolder;

        Configuration<CL2M9Agent> _configuration;


        //statistics
        List<AgentNumericValuesOutput> _variableStatistic;
        


        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                RuleSelectionEnabled = true,
                RuleSelectionPart2Enabled = true,
                AgentRandomizationEnabled = true,
            };
        }

        public CL2M9Algorithm(Configuration<CL2M9Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _variableStatistic = new List<AgentNumericValuesOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL2_M9";

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
            StatisticHelper.Save(_variableStatistic, $@"{_outputFolder}\variable_statistic.csv");
        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();

            agent.SetToCommon(VariablesUsedInCode.Iteration, iteration);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();

            agent.SetToCommon(VariablesUsedInCode.CommonPoolC, _agentList.CalculateCommonC());
            agent.SetToCommon(VariablesUsedInCode.CommonPoolSize, _agentList.Agents.Count);

            agent.SetToCommon(VariablesUsedInCode.PoolWellbeing, CalculatePoolWellbeing(agent));

            orderedAgents.AsParallel().ForAll(a =>
            {
                a[VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);
            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;

            _variableStatistic.Add(StatisticHelper.CreateAgentValuesRecord(activeAgents, iteration, VariablesUsedInCode.AgentC));
        }


        private double CalculateAgentWellbeing(IAgent agent)
        {
            return agent[VariablesUsedInCode.Endowment] - agent[VariablesUsedInCode.AgentC]
                + agent[VariablesUsedInCode.MagnitudeOfExternalities] * agent[VariablesUsedInCode.CommonPoolC] / (double)agent[VariablesUsedInCode.CommonPoolSize];
        }

        private double CalculatePoolWellbeing(IAgent agent)
        {
            return agent[VariablesUsedInCode.MagnitudeOfExternalities] * agent[VariablesUsedInCode.CommonPoolC] / (double)agent[VariablesUsedInCode.CommonPoolSize];
        }
    }
}
