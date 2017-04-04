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
    public sealed class CL4M11Algorithm : SosielAlgorithm<CL4M11Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 4 Model 11"; } }

        string _outputFolder;

        Configuration<CL4M11Agent> _configuration;

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

        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            return IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents);
        }

        protected override void AfterInitialization()
        {
            StatisticHelper.SaveState(_outputFolder, "initial", _agentList, _siteList);

        }

        protected override void AfterAlgorithmExecuted()
        {
            StatisticHelper.SaveState(_outputFolder, "final", _agentList, _siteList);
        }

        protected override void PreIterationCalculations(int iteration)
        {
            base.PreIterationCalculations(iteration);

            _agentList.Agents.First().SetToCommon(Agent.VariablesUsedInCode.Iteration, iteration);
        }

        protected override void PostIterationCalculations(int iteration)
        {
            base.PostIterationCalculations(iteration);

            IAgent agent = _agentList.Agents.First();

            UpdateEndowment();

            _agentList.Agents.ForEach(a =>
            {
                a.ConnectedAgents = _siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentCurrentSite]).Where(s => s.IsOccupied)
                    .Select(s => s.OccupiedBy).ToList();

                a[Agent.VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentWellbeing(a);
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
            int commonPoolC = agent.ConnectedAgents.Sum(a=>a[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.AgentE] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)agent.ConnectedAgents.Count + 1);
        }

        private double CalculateAgentWellbeing(IAgent agent, Site centerSite)
        {
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.AgentE] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);
        }

        
        private void UpdateEndowment()
        {
            IAgent agent = _agentList.Agents.First();

            int totalE = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active")
                .Sum(a => (int)a[Agent.VariablesUsedInCode.AgentE]);

            //E(t) == ( r ^ p ) * ( E(t-1) – total_e(t-1) )
            int endowment = Math.Pow(agent[Agent.VariablesUsedInCode.R], agent[Agent.VariablesUsedInCode.P]) * (agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.TotalEndowment]);

            agent.SetToCommon(Agent.VariablesUsedInCode.TotalEndowment, totalE);
            agent.SetToCommon(Agent.VariablesUsedInCode.Endowment, endowment);
        }


        RuleFrequenciesOutput CreateRuleFrequenciesRecord(int iteration, Dictionary<IAgent, AgentState> iterationState)
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
