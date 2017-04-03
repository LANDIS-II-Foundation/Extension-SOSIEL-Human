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

namespace CL4_M11
{
    public sealed class CL4M11Algorithm : SosielAlgorithm, IAlgorithm
    {
        public string Name { get { return "Cognitive level 4 Model 11"; } }

        string _outputFolder;

        Configuration<CL4M11Agent> _configuration;

        LinkedList<Dictionary<IConfigurableAgent, AgentState>> _iterations = new LinkedList<Dictionary<IConfigurableAgent, AgentState>>();

        List<AgentContributionsOutput> _agentContributionsStatistic;
        List<RuleFrequenciesOutput> _ruleFrequenciesStatistic;

        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
                {
                    ActionTakingEnabled = true,
                    AnticipatoryLearningEnabled = true,
                    RuleSelectionEnabled = true,
                    RuleSelectionPart2Enabled = true,
                    SocialLearningEnabled = true,
                    CounterfactualThinkingEnabled = true,
                    InnovationEnabled = true,
                    ReproductionEnabled = true
                };
        }

        public CL4M11Algorithm(Configuration<CL4M11Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;
            
            _agentContributionsStatistic = new List<AgentContributionsOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _ruleFrequenciesStatistic = new List<RuleFrequenciesOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            
            _outputFolder = @"Output\CL4_M11";

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
            _siteList = SiteList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AlgorithmConfiguration.VacantProportion);

            _agentList = AgentList.Generate2(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AgentConfiguration, _configuration.InitialState, _siteList);
        }

        protected override Dictionary<IConfigurableAgent, AgentState> InitializeFirstIterationState()
        {
            return IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents.Cast<IConfigurableAgent>());
        }

        protected override void PreIterationCalculations(int iteration)
        {
            base.PreIterationCalculations(iteration);

            _agentList.Agents.ForEach(a =>
            {
                a[Agent.VariablesUsedInCode.Iteration] = iteration;
            });
        }

        protected override void PostIterationCalculations(int iteration)
        {
            base.PostIterationCalculations(iteration);

            double poolWellbeing = CalculateCommonPoolWellbeing(_agentList.Agents.First()[Agent.VariablesUsedInCode.MagnitudeOfExternalities]);

            _agentList.Agents.ForEach(a =>
            {
                a[Agent.VariablesUsedInCode.PoolWellbeing] = poolWellbeing;
                a[Agent.VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);
            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            _agentContributionsStatistic.Add(new AgentContributionsOutput { Iteration = iteration, AgentContributions = _agentList.Agents.Select(a => (double)a[Agent.VariablesUsedInCode.AgentC]).ToArray() });
            _ruleFrequenciesStatistic.Add(CreateRuleFrequenciesRecord(iteration, _iterations.Last.Value));
        }


        private double CalculateAgentWellbeing(IAgent agent)
        {
            return agent[Agent.VariablesUsedInCode.Engage] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * _agentList.CalculateCommonC() / (double)_agentList.Agents.Count;
        }

        private double CalculateCommonPoolWellbeing(double externalities)
        {
            return externalities * _agentList.CalculateCommonC() / (double)_agentList.Agents.Count;
        }



        RuleFrequenciesOutput CreateRuleFrequenciesRecord(int iteration, Dictionary<IConfigurableAgent, AgentState> iterationState)
        {
            //todo

            //List<Rule> allRules = _agentList.Agents.First().Rules;

            RuleFrequenceItem[] items = iterationState.SelectMany(kvp => kvp.Value.Activated).GroupBy(r => r.Id)
                .Select(g => new RuleFrequenceItem { RuleId = g.Key, Frequence = g.Count() }).ToArray();



            return new RuleFrequenciesOutput { Iteration = iteration, RuleFrequencies = items };


        }

        void SaveAgentWellbeingStatistic()
        {
            ResultSavingHelper.Save(_agentContributionsStatistic, $@"{_outputFolder}\contributions_statistic.csv");
        }

        void SaveRuleFrequenceStatistic()
        {
            ResultSavingHelper.Save(_ruleFrequenciesStatistic, $@"{_outputFolder}\rule_frequencies_statistic.csv");
        }
    }
}
