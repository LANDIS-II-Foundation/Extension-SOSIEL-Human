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

namespace CL3_M10
{
    public sealed class CL3M10Algorithm : SosielAlgorithm<CL3M10Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 3 Model 10"; } }

        string _outputFolder;

        Configuration<CL3M10Agent> _configuration;


        //statistics
        List<AgentNumericValuesOutput> _variableStatistic;
        List<RuleFrequenciesOutput> _ruleFrequencies;


        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                RuleSelectionEnabled = true,
                RuleSelectionPart2Enabled = true,
                SocialLearningEnabled = true,
                AgentRandomizationEnabled = true,
            };
        }

        public CL3M10Algorithm(Configuration<CL3M10Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _variableStatistic = new List<AgentNumericValuesOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _ruleFrequencies = new List<RuleFrequenciesOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL3_M10";

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
            StatisticHelper.Save(_ruleFrequencies, $@"{_outputFolder}\rule_frequency.csv");
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
            agent.SetToCommon(Agent.VariablesUsedInCode.CommonPoolSize, _agentList.Agents.Count); 

            agent.SetToCommon(Agent.VariablesUsedInCode.PoolWellbeing, CalculatePoolWellbeing(agent));

            orderedAgents.AsParallel().ForAll(a =>
            {
                a[Agent.VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);
            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;

            _variableStatistic.Add(StatisticHelper.CreateAgentValuesRecord(activeAgents, iteration, Agent.VariablesUsedInCode.AgentC));
            _ruleFrequencies.Add(StatisticHelper.CreateRuleFrequenciesRecord(activeAgents, iteration));
        }


        private double CalculateAgentWellbeing(IAgent agent)
        {
            return agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * agent[Agent.VariablesUsedInCode.CommonPoolC] / (double)agent[Agent.VariablesUsedInCode.CommonPoolSize];
        }

        private double CalculatePoolWellbeing(IAgent agent)
        {
            return agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * agent[Agent.VariablesUsedInCode.CommonPoolC] / (double)agent[Agent.VariablesUsedInCode.CommonPoolSize];
        }
    }
}
