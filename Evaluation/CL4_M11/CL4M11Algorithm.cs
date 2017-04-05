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


        //statistics
        List<SubtypeProportionOutput> _subtypeProportionStatistic;
        List<CommonPoolSubtypeFrequencyOutput> _commonPoolFrequencyStatistic;
        List<ValuesOutput> _valuesOutput;


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
                    ReproductionEnabled = true,
                    AgentRandomizationEnabled = true,
                    AgentsDeactivationEnabled = true
                };
        }

        public CL4M11Algorithm(Configuration<CL4M11Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _commonPoolFrequencyStatistic = new List<CommonPoolSubtypeFrequencyOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _valuesOutput = new List<ValuesOutput>(_configuration.AlgorithmConfiguration.IterationCount);

            
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
            StatisticHelper.SaveState(_outputFolder, "initial", _agentList.ActiveAgents, _siteList);


            _subtypeProportionStatistic.Add(StatisticHelper.CreateSubtypeProportionRecord(_agentList.ActiveAgents, 0, (int)AgentSubtype.Co));
        }

        protected override void AfterAlgorithmExecuted()
        {
            StatisticHelper.SaveState(_outputFolder, "final", _agentList.ActiveAgents, _siteList);



        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            _agentList.Agents.First().SetToCommon(Agent.VariablesUsedInCode.Iteration, iteration);

            LookingForBetterSites(orderedAgents);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = _agentList.Agents.First();

            UpdateEndowment();

            orderedAgents.AsParallel().ForAll(a =>
            {
                Site currentSite = a[Agent.VariablesUsedInCode.AgentCurrentSite];

                a.ConnectedAgents = _siteList.AdjacentSites(currentSite).Where(s => s.IsOccupied)
                    .Select(s => s.OccupiedBy).ToList();

                a[Agent.VariablesUsedInCode.AgentSubtype] = a[Agent.VariablesUsedInCode.AgentC] > 0 ? AgentSubtype.Co : AgentSubtype.NonCo;


                a[Agent.VariablesUsedInCode.NeighborhoodSize] = currentSite.GroupSize;
                a[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = currentSite.GroupSize - a.ConnectedAgents.Count;


                a[Agent.VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[Agent.VariablesUsedInCode.AgentSubtype] != a[Agent.VariablesUsedInCode.AgentSubtype]);
                a[Agent.VariablesUsedInCode.CommonPoolSize] = a[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - a[Agent.VariablesUsedInCode.NeighborhoodVacantSites];

                a[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = (a[Agent.VariablesUsedInCode.CommonPoolSize] - a[Agent.VariablesUsedInCode.CommonPoolUnalike]) / (double)a[Agent.VariablesUsedInCode.CommonPoolSize];

                a[Agent.VariablesUsedInCode.CommonPoolC] = a.ConnectedAgents.Sum(a2 => a2[Agent.VariablesUsedInCode.AgentC]) + a[Agent.VariablesUsedInCode.AgentC];

                

                a[Agent.VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentWellbeing(a);

                a[Agent.VariablesUsedInCode.PoolWellbeing] = CalculatePoolWellbeing(a);

                a[Agent.VariablesUsedInCode.AgentSavings] = a[Agent.VariablesUsedInCode.AgentE] - a[Agent.VariablesUsedInCode.AgentC];
            });



        }


        protected override void AgentsDeactivation()
        {
            base.AgentsDeactivation();

            _agentList.ActiveAgents.ForEach(a =>
            {
                if(a[Agent.VariablesUsedInCode.AgentSiteWellbeing] <= 0)
                {
                    a[Agent.VariablesUsedInCode.AgentStatus] = "inactive";
                }
            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;

            //subtype proportions
            _subtypeProportionStatistic.Add(StatisticHelper.CreateSubtypeProportionRecord(activeAgents, iteration, (int)AgentSubtype.Co));
            //frequency
            _commonPoolFrequencyStatistic.Add(StatisticHelper.CreateCommonPoolFrequencyRecord(activeAgents, iteration, (int)AgentSubtype.Co));
            //params
            ValuesOutput valuesRecord = StatisticHelper.CreateValuesRecord(activeAgents, iteration,
                Agent.VariablesUsedInCode.Endowment,
                $"AVG_{Agent.VariablesUsedInCode.AgentE}",
                $"AVG_{Agent.VariablesUsedInCode.AgentC}"
                );

            valuesRecord.Values.Add(new ValueItem { Name = "N", Value = activeAgents.Length });


            _valuesOutput.Add(valuesRecord);
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

        private double CalculatePoolWellbeing(IAgent agent)
        {
            int commonPoolC = agent.ConnectedAgents.Sum(a => a[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)agent.ConnectedAgents.Count + 1);
        }

        private void LookingForBetterSites(IAgent[] orderedAgent)
        {
            List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

            orderedAgent.ForEach(agent =>
            {

                Site[] betterSites = vacantSites.AsParallel()
                        .Select(site => new
                        {
                            site,
                            Wellbeing = CalculateAgentWellbeing(agent, site)
                        })
                        .Where(obj => obj.Wellbeing > agent[Agent.VariablesUsedInCode.AgentSiteWellbeing]).AsSequential()
                        .GroupBy(obj => obj.Wellbeing).OrderByDescending(obj => obj.Key)
                        .Take(1).SelectMany(g => g.Select(o => o.site)).ToArray();

                if (betterSites.Length > 0)
                {
                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = true;

                    Site currentSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];
                    Site selectedSite = betterSites.RandomizeOne();
                    agent[Agent.VariablesUsedInCode.AgentBetterSite] = selectedSite;

                    vacantSites.Add(currentSite);
                    vacantSites.Remove(selectedSite);
                };
            });
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

    }
}
