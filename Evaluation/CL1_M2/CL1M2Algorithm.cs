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

namespace CL1_M2
{
    public sealed class CL1M2Algorithm : SosielAlgorithm<CL1M2Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 1 Model 2"; } }

        string _outputFolder;

        Configuration<CL1M2Agent> _configuration;


        //statistics
        List<SubtypeProportionOutput> _subtypeProportionStatistic;
        List<AvgWellbeingOutput> _averageWellbeing;
        List<DebugAgentsPositionOutput> _debugSiteOutput;

        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                RuleSelectionEnabled = true,
                AgentRandomizationEnabled = true,
                AlgorithmStopIfAllAgentsSelectDoNothing = true
            };
        }

        public CL1M2Algorithm(Configuration<CL1M2Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _averageWellbeing = new List<AvgWellbeingOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _debugSiteOutput = new List<DebugAgentsPositionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL1_M2";

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
            StatisticHelper.Save(_averageWellbeing, $@"{_outputFolder}\subtype_wellbeing_statistic.csv");

            StatisticHelper.Save(_debugSiteOutput, $@"{_outputFolder}\debug.csv");
        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            IAgent agent = _agentList.Agents.First();

            agent.SetToCommon(Agent.VariablesUsedInCode.Iteration, iteration);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();


            orderedAgents.AsParallel().ForAll(a =>
            {
                Site currentSite = a[Agent.VariablesUsedInCode.AgentCurrentSite];

                if (currentSite.IsOccupationChanged)
                {
                    a.ConnectedAgents = _siteList.AdjacentSites(currentSite).Where(s => s.IsOccupied)
                        .Select(s => s.OccupiedBy).ToList();

                    a[Agent.VariablesUsedInCode.NeighborhoodSize] = currentSite.GroupSize;
                    a[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = currentSite.GroupSize - a.ConnectedAgents.Count;


                    a[Agent.VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[Agent.VariablesUsedInCode.AgentSubtype] != a[Agent.VariablesUsedInCode.AgentSubtype]);
                    a[Agent.VariablesUsedInCode.CommonPoolSize] = a[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - a[Agent.VariablesUsedInCode.NeighborhoodVacantSites];

                    a[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = (a[Agent.VariablesUsedInCode.CommonPoolSize] - a[Agent.VariablesUsedInCode.CommonPoolUnalike]) / (double)a[Agent.VariablesUsedInCode.CommonPoolSize];

                    a[Agent.VariablesUsedInCode.CommonPoolC] = a.ConnectedAgents.Sum(a2 => a2[Agent.VariablesUsedInCode.AgentC]) + a[Agent.VariablesUsedInCode.AgentC];
                }

                a[Agent.VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentSiteWellbeing(a);
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

            _averageWellbeing.Add(StatisticHelper.CreateAvgWellbeingStatisticRecord(activeAgents, iteration));

            _debugSiteOutput.Add(StatisticHelper.CreateDebugAgentsPositionRecord(_siteList, iteration));
        }


        private double CalculateAgentSiteWellbeing(IAgent agent)
        {
            Site currentSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];

            double wellbeing = agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * agent[Agent.VariablesUsedInCode.CommonPoolC] / (double)agent[Agent.VariablesUsedInCode.CommonPoolSize];

            return wellbeing;
        }

        private double CalculateAgentSiteWellbeing(IAgent agent, Site centerSite)
        {
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied && s != agent[Agent.VariablesUsedInCode.AgentCurrentSite]).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];


            double wellbeing = agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);

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
                    .Where(obj => obj.Wellbeing > agent[Agent.VariablesUsedInCode.AgentSiteWellbeing]).AsSequential()
                    .GroupBy(obj => obj.Wellbeing).OrderByDescending(obj => obj.Key)
                    .Take(1).SelectMany(g => g.Select(o => o.site)).ToArray();

            if (betterSites.Length > 0)
            {
                agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = true;

                Site currentSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];
                Site selectedSite = betterSites.RandomizeOne();
                agent[Agent.VariablesUsedInCode.AgentBetterSite] = selectedSite;
            }
            else
            {
                agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = false;
            }
        }
    }
}
