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

namespace CL1_M5
{
    public sealed class CL1M5Algorithm : SosielAlgorithm<CL1M5Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 1 Model 5"; } }

        string _outputFolder;

        Configuration<CL1M5Agent> _configuration;


        //statistics
        List<SubtypeProportionOutput> _subtypeProportionStatistic;
        List<CommonPoolSubtypeFrequencyWithDisturbanceOutput> _commonPoolSubtypeFrequency;


        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                RuleSelectionEnabled = true,
                AgentRandomizationEnabled = true,
                ReproductionEnabled = true,
                AgentsDeactivationEnabled = true,
                AlgorithmStopIfAllAgentsSelectDoNothing = true
            };
        }

        public CL1M5Algorithm(Configuration<CL1M5Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _commonPoolSubtypeFrequency = new List<CommonPoolSubtypeFrequencyWithDisturbanceOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL1_M5";

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
            numberOfAgents = _configuration.InitialState.AgentsState.Sum(astate => astate.NumberOfAgents);

            _siteList = SiteList.Generate(numberOfAgents, _configuration.AlgorithmConfiguration.VacantProportion);

            _agentList = AgentList.Generate(numberOfAgents, _configuration.AgentConfiguration, _configuration.InitialState, _siteList);
        }

        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            return IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents);
        }

        protected override void AfterInitialization()
        {
            StatisticHelper.SaveState(_outputFolder, "initial", _agentList.ActiveAgents, _siteList);
        }

        protected override void AfterAlgorithmExecuted()
        {
            StatisticHelper.SaveState(_outputFolder, "final", _agentList.ActiveAgents, _siteList);

            StatisticHelper.Save(_subtypeProportionStatistic, $@"{_outputFolder}\subtype_proportion_statistic.csv");
            StatisticHelper.Save(_commonPoolSubtypeFrequency, $@"{_outputFolder}\common_pool_frequncy_statistic.csv");
        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            IAgent agent = _agentList.Agents.First();

            agent.SetToCommon(Agent.VariablesUsedInCode.Iteration, iteration);

            agent.SetToCommon(Agent.VariablesUsedInCode.Disturbance, agent[Agent.VariablesUsedInCode.Disturbance] + agent[Agent.VariablesUsedInCode.DisturbanceIncrement]);

            if (iteration > 1)
                LookingForBetterSites(orderedAgents);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = _agentList.Agents.First();


            orderedAgents.AsParallel().ForAll(a =>
            {
                Site currentSite = a[Agent.VariablesUsedInCode.AgentCurrentSite];

                a.ConnectedAgents = _siteList.AdjacentSites(currentSite).Where(s => s.IsOccupied)
                    .Select(s => s.OccupiedBy).ToList();


                a[Agent.VariablesUsedInCode.NeighborhoodSize] = currentSite.GroupSize;
                a[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = currentSite.GroupSize - a.ConnectedAgents.Count;


                a[Agent.VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[Agent.VariablesUsedInCode.AgentSubtype] != a[Agent.VariablesUsedInCode.AgentSubtype]);
                a[Agent.VariablesUsedInCode.CommonPoolSize] = a[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - a[Agent.VariablesUsedInCode.NeighborhoodVacantSites];

                a[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = (a[Agent.VariablesUsedInCode.CommonPoolSize] - a[Agent.VariablesUsedInCode.CommonPoolUnalike]) / (double)a[Agent.VariablesUsedInCode.CommonPoolSize];

                a[Agent.VariablesUsedInCode.CommonPoolC] = a.ConnectedAgents.Sum(a2 => a2[Agent.VariablesUsedInCode.AgentC]) + a[Agent.VariablesUsedInCode.AgentC];



                a[Agent.VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentSiteWellbeing(a);

                if (a[Agent.VariablesUsedInCode.AgentSubtype] != AgentSubtype.NonCo)
                {
                    a[Agent.VariablesUsedInCode.AgentSubtype] = (a[Agent.VariablesUsedInCode.AgentC] + a[Agent.VariablesUsedInCode.AgentP])
                        > a[Agent.VariablesUsedInCode.AgentSiteWellbeing] ? AgentSubtype.StrCo : AgentSubtype.WeakCo;
                }

            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;
            IAgent agent = activeAgents.First();


            SubtypeProportionOutput spo = StatisticHelper.CreateCommonPoolSubtypeProportionRecord(activeAgents, iteration, (int)AgentSubtype.StrCo);
            spo.Subtype = EnumHelper.EnumValueAsString(AgentSubtype.StrCo);
            _subtypeProportionStatistic.Add(spo);

            _commonPoolSubtypeFrequency.Add(StatisticHelper.CreateCommonPoolFrequencyWithDisturbanceRecord(activeAgents, iteration, (int)AgentSubtype.StrCo, agent[Agent.VariablesUsedInCode.Disturbance]));
        }

        protected override void AgentsDeactivation()
        {
            base.AgentsDeactivation();

            _agentList.ActiveAgents.ForEach(a =>
            {
                if (a[Agent.VariablesUsedInCode.AgentSiteWellbeing] <= 0)
                {
                    a[Agent.VariablesUsedInCode.AgentStatus] = "inactive";
                    a[Agent.VariablesUsedInCode.AgentCurrentSite] = null;
                }
            });
        }


        private double CalculateAgentSiteWellbeing(IAgent agent)
        {
            Site currentSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];

            double wellbeing = agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * agent[Agent.VariablesUsedInCode.CommonPoolC] / (double)agent[Agent.VariablesUsedInCode.CommonPoolSize] - agent[Agent.VariablesUsedInCode.Disturbance];

            double penalties = agent.ConnectedAgents.Sum(e => (double)(e[Agent.VariablesUsedInCode.AgentP] * (1 - agent[Agent.VariablesUsedInCode.AgentC] / (double)agent[Agent.VariablesUsedInCode.Endowment])));

            wellbeing -= penalties;

            double punishment = agent.ConnectedAgents.Sum(n => (double)(agent[Agent.VariablesUsedInCode.AgentP] * (1 - n[Agent.VariablesUsedInCode.AgentC] / (double)agent[Agent.VariablesUsedInCode.Endowment])));

            wellbeing -= punishment;


            return wellbeing;
        }

        private double CalculateAgentSiteWellbeing(IAgent agent, Site centerSite)
        {
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];


            double wellbeing = agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1) - agent[Agent.VariablesUsedInCode.Disturbance];

            double penalties = commonPool.Select(s => s.OccupiedBy).Sum(e => (double)(e[Agent.VariablesUsedInCode.AgentP] * (1 - agent[Agent.VariablesUsedInCode.AgentC] / (double)agent[Agent.VariablesUsedInCode.Endowment])));

            wellbeing -= penalties;

            double punishment = commonPool.Select(s => s.OccupiedBy).Sum(n => (double)(agent[Agent.VariablesUsedInCode.AgentP] * (1 - n[Agent.VariablesUsedInCode.AgentC] / (double)agent[Agent.VariablesUsedInCode.Endowment])));

            wellbeing -= punishment;


            return wellbeing;
        }

        private void LookingForBetterSites(IAgent[] orderedAgents)
        {
            List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

            orderedAgents.ForEach(agent =>
            {

                Site[] betterSites = vacantSites.AsParallel()
                        .Select(site => new
                        {
                            site,
                            Wellbeing = CalculateAgentSiteWellbeing(agent, site)
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
                }
                else
                {
                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = false;
                }
            });
        }
    }
}
