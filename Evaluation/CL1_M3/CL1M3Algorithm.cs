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

namespace CL1_M3
{
    public sealed class CL1M3Algorithm : SosielAlgorithm<CL1M3Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 1 Model 3"; } }

        string _outputFolder;

        Configuration<CL1M3Agent> _configuration;


        //statistics
        List<SubtypeProportionOutput> _subtypeProportionStatistic;
        List<CommonPoolSubtypeFrequencyWithDisturbanceOutput> _commonPoolSubtypeFrequency;
        //List<DebugAgentsPositionOutput> _debugSiteOutput;


        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                RuleSelectionEnabled = true,
                AgentRandomizationEnabled = true,
                AgentsDeactivationEnabled = true,
                AlgorithmStopIfAllAgentsSelectDoNothing = true
            };
        }

        public CL1M3Algorithm(Configuration<CL1M3Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _commonPoolSubtypeFrequency = new List<CommonPoolSubtypeFrequencyWithDisturbanceOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            //_debugSiteOutput = new List<DebugAgentsPositionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL1_M3";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);

            _preliminaryCalculations.Add("BetterSite", new Action<IAgent>(LookingForBetterSite));
        }

        public string Run()
        {
            ExecuteAlgorithm();

            return _outputFolder;
        }

        protected override void InitializeAgents()
        {
            _numberOfAgents = _configuration.InitialState.AgentsState.Sum(astate => astate.NumberOfAgents);

            _siteList = SiteList.Generate(_numberOfAgents, _configuration.AlgorithmConfiguration.VacantProportion);

            _agentList = AgentList.Generate(_numberOfAgents, _configuration.AgentConfiguration, _configuration.InitialState, _siteList);
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

            //StatisticHelper.Save(_debugSiteOutput, $@"{_outputFolder}\debug.csv");
        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();

            agent.SetToCommon(VariablesUsedInCode.Iteration, iteration);

            agent.SetToCommon(VariablesUsedInCode.Disturbance, agent[VariablesUsedInCode.Disturbance] + agent[VariablesUsedInCode.DisturbanceIncrement]);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();


            orderedAgents.AsParallel().ForAll(a =>
            {
                Site currentSite = a[VariablesUsedInCode.AgentCurrentSite];

                if (currentSite.IsOccupationChanged)
                {
                    a.ConnectedAgents = _siteList.AdjacentSites(currentSite).Where(s => s.IsOccupied)
                        .Select(s => s.OccupiedBy).ToList();


                    a[VariablesUsedInCode.NeighborhoodSize] = currentSite.GroupSize;
                    a[VariablesUsedInCode.NeighborhoodVacantSites] = currentSite.GroupSize - a.ConnectedAgents.Count;


                    a[VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[VariablesUsedInCode.AgentSubtype] != a[VariablesUsedInCode.AgentSubtype]);
                    a[VariablesUsedInCode.CommonPoolSize] = a[VariablesUsedInCode.NeighborhoodSize] + 1 - a[VariablesUsedInCode.NeighborhoodVacantSites];

                    a[VariablesUsedInCode.CommonPoolSubtupeProportion] = (a[VariablesUsedInCode.CommonPoolSize] - a[VariablesUsedInCode.CommonPoolUnalike]) / (double)a[VariablesUsedInCode.CommonPoolSize];

                    a[VariablesUsedInCode.CommonPoolC] = a.ConnectedAgents.Sum(a2 => a2[VariablesUsedInCode.AgentC]) + a[VariablesUsedInCode.AgentC];
                }


                a[VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentSiteWellbeing(a);

            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;
            IAgent agent = activeAgents.First();


            SubtypeProportionOutput spo = StatisticHelper.CreateCommonPoolSubtypeProportionRecord(activeAgents, iteration, (int)AgentSubtype.Co);
            spo.Subtype = EnumHelper.EnumValueAsString(AgentSubtype.Co);
            _subtypeProportionStatistic.Add(spo);

            _commonPoolSubtypeFrequency.Add(StatisticHelper.CreateCommonPoolFrequencyWithDisturbanceRecord(activeAgents, iteration, (int)AgentSubtype.Co, agent[VariablesUsedInCode.Disturbance]));


            StatisticHelper.SaveState(_outputFolder, iteration.ToString(), _agentList.ActiveAgents, _siteList);

            //_debugSiteOutput.Add(StatisticHelper.CreateDebugAgentsPositionRecord(_siteList, iteration));
        }

        protected override void AgentsDeactivation()
        {
            base.AgentsDeactivation();

            _agentList.ActiveAgents.ForEach(a =>
            {
                if (a[VariablesUsedInCode.AgentSiteWellbeing] <= 0)
                {
                    a[VariablesUsedInCode.AgentStatus] = "inactive";
                    a[VariablesUsedInCode.AgentCurrentSite] = null;
                }
            });
        }

        private double CalculateAgentSiteWellbeing(IAgent agent)
        {
            Site currentSite = agent[VariablesUsedInCode.AgentCurrentSite];

            double wellbeing = agent[VariablesUsedInCode.Endowment] - agent[VariablesUsedInCode.AgentC]
                + agent[VariablesUsedInCode.MagnitudeOfExternalities] * agent[VariablesUsedInCode.CommonPoolC] / (double)agent[VariablesUsedInCode.CommonPoolSize] - agent[VariablesUsedInCode.Disturbance];

            return wellbeing;
        }

        private double CalculateAgentSiteWellbeing(IAgent agent, Site centerSite)
        {
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied && s != agent[VariablesUsedInCode.AgentCurrentSite]).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[VariablesUsedInCode.AgentC]) + agent[VariablesUsedInCode.AgentC];


            double wellbeing = agent[VariablesUsedInCode.Endowment] - agent[VariablesUsedInCode.AgentC]
                + agent[VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1) - agent[VariablesUsedInCode.Disturbance];

            return wellbeing;
        }

        private void LookingForBetterSite(IAgent agent)
        {
            List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

            Site[] betterSites = vacantSites.AsParallel()
                    .Select(site => new
                    {
                        site,
                        Wellbeing = CalculateAgentSiteWellbeing(agent, site)
                    })
                    .Where(obj => obj.Wellbeing > agent[VariablesUsedInCode.AgentSiteWellbeing]).AsSequential()
                    .GroupBy(obj => obj.Wellbeing).OrderByDescending(obj => obj.Key)
                    .Take(1).SelectMany(g => g.Select(o => o.site)).ToArray();

            if (betterSites.Length > 0)
            {
                agent[VariablesUsedInCode.AgentBetterSiteAvailable] = true;

                Site currentSite = agent[VariablesUsedInCode.AgentCurrentSite];
                Site selectedSite = betterSites.RandomizeOne();
                agent[VariablesUsedInCode.AgentBetterSite] = selectedSite;
            }
            else
            {
                agent[VariablesUsedInCode.AgentBetterSiteAvailable] = false;
            }
        }
    }
}
