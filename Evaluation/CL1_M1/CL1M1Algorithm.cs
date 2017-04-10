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
using Common.Models;

namespace CL1_M1
{
    public sealed class CL1M1Algorithm : SosielAlgorithm<CL1M1Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 1 Model 1"; } }

        string _outputFolder;

        Configuration<CL1M1Agent> _configuration;


        //statistics
        List<SubtypeProportionOutput> _subtypeProportionStatistic;
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

        public CL1M1Algorithm(Configuration<CL1M1Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _debugSiteOutput = new List<DebugAgentsPositionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);

            _outputFolder = @"Output\CL1_M1";

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

            StatisticHelper.Save(_debugSiteOutput, $@"{_outputFolder}\debug.csv");
        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();

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
                    a[Agent.VariablesUsedInCode.NeighborhoodUnalike] = a[Agent.VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[Agent.VariablesUsedInCode.AgentSubtype] != a[Agent.VariablesUsedInCode.AgentSubtype]);

                    a[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion] = (a[Agent.VariablesUsedInCode.NeighborhoodSize] -
                        a[Agent.VariablesUsedInCode.NeighborhoodVacantSites] - a[Agent.VariablesUsedInCode.NeighborhoodUnalike]) / (double)a[Agent.VariablesUsedInCode.NeighborhoodSize];

                    currentSite.IsOccupationChanged = false;
                }
            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;
            IAgent agent = activeAgents.First();


            SubtypeProportionOutput spo = StatisticHelper.CreateNeighbourhoodSubtypeProportionRecord(activeAgents, iteration, (int)AgentSubtype.TypeA);
            spo.Subtype = EnumHelper.EnumValueAsString(AgentSubtype.TypeA);
            _subtypeProportionStatistic.Add(spo);

            _debugSiteOutput.Add(StatisticHelper.CreateDebugAgentsPositionRecord(_siteList, iteration));
        }

        private double CalculateSubtypeProportion(IAgent agent, Site centerSite)
        {
            IAgent[] neighbours = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied && s != agent[Agent.VariablesUsedInCode.AgentCurrentSite]).Select(s => s.OccupiedBy).ToArray();

            return neighbours.Count(a => a[Agent.VariablesUsedInCode.AgentSubtype] == agent[Agent.VariablesUsedInCode.AgentSubtype]) / (double)centerSite.GroupSize;
        }

        private void LookingForBetterSite(IAgent agent)
        {
            List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

            Site[] betterSites = vacantSites.AsParallel()
                    .Select(site => new
                    {
                        site,
                        Proportion = CalculateSubtypeProportion(agent, site)
                    })
                    .Where(obj => obj.Proportion > agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion]).AsSequential()
                    .GroupBy(obj => obj.Proportion).OrderByDescending(obj => obj.Key)
                    .Take(1).SelectMany(g => g.Select(o => o.site))
                    .Select(site => new
                    {
                        site,
                        Proximity = site.DistanceToAnotherSite(agent[Agent.VariablesUsedInCode.AgentCurrentSite])
                    })
                    .GroupBy(obj => obj.Proximity).OrderBy(g => g.Key)
                    .Take(1).SelectMany(g => g.Select(o => o.site))
                    .ToArray();


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
