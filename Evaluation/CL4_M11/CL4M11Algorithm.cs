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
        List<CommonValuesOutput> _valuesOutput;


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
                AgentsDeactivationEnabled = true,
                AlgorithmStopIfAllAgentsSelectDoNothing = true
            };
        }

        public CL4M11Algorithm(Configuration<CL4M11Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _commonPoolFrequencyStatistic = new List<CommonPoolSubtypeFrequencyOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);
            _valuesOutput = new List<CommonValuesOutput>(_configuration.AlgorithmConfiguration.NumberOfIterations);


            _outputFolder = @"Output\CL4_M11";

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


            _subtypeProportionStatistic.Add(StatisticHelper.CreateCommonPoolSubtypeProportionRecord(_agentList.ActiveAgents, 0, (int)AgentSubtype.Co));
        }

        protected override void AfterAlgorithmExecuted()
        {
            StatisticHelper.SaveState(_outputFolder, "final", _agentList.ActiveAgents, _siteList);

            StatisticHelper.Save(_subtypeProportionStatistic, $@"{_outputFolder}\subtype_proportion_statistic.csv");
            StatisticHelper.Save(_commonPoolFrequencyStatistic, $@"{_outputFolder}\common_pool_frequncy_statistic.csv");
            StatisticHelper.Save(_valuesOutput, $@"{_outputFolder}\values_statistic.csv");

        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            IAgent agent = orderedAgents.First();

            agent.SetToCommon(VariablesUsedInCode.Iteration, iteration);

            if(iteration > 1)
            {
                UpdateEndowment();
            }
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
                    a.ConnectedAgents = _siteList.AdjacentSites(currentSite).Where(s => s.IsOccupied && s != agent[VariablesUsedInCode.AgentCurrentSite])
                        .Select(s => s.OccupiedBy).ToList();

                    a[VariablesUsedInCode.NeighborhoodSize] = currentSite.GroupSize;
                    a[VariablesUsedInCode.NeighborhoodVacantSites] = currentSite.GroupSize - a.ConnectedAgents.Count;

                    a[VariablesUsedInCode.CommonPoolSize] = a[VariablesUsedInCode.NeighborhoodSize] + 1 - a[VariablesUsedInCode.NeighborhoodVacantSites];
                }

                a[VariablesUsedInCode.AgentSubtype] = a[VariablesUsedInCode.AgentC] > 0 ? AgentSubtype.Co : AgentSubtype.NonCo;

                a[VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[VariablesUsedInCode.AgentSubtype] != a[VariablesUsedInCode.AgentSubtype]);
                a[VariablesUsedInCode.CommonPoolSubtupeProportion] = (a[VariablesUsedInCode.CommonPoolSize] - a[VariablesUsedInCode.CommonPoolUnalike]) / (double)a[VariablesUsedInCode.CommonPoolSize];

                a[VariablesUsedInCode.CommonPoolC] = a.ConnectedAgents.Sum(a2 => a2[VariablesUsedInCode.AgentC]) + a[VariablesUsedInCode.AgentC];

                a[VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentSiteWellbeing(a);

                a[VariablesUsedInCode.PoolWellbeing] = CalculatePoolWellbeing(a);

                a[VariablesUsedInCode.AgentSavings] = a[VariablesUsedInCode.AgentE] - a[VariablesUsedInCode.AgentC];

                a[VariablesUsedInCode.MaxPoolWellbeing] = a[VariablesUsedInCode.MagnitudeOfExternalities] * a[VariablesUsedInCode.Endowment] / a[VariablesUsedInCode.CommonPoolSize];
            });
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

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;

            //subtype proportions
            SubtypeProportionOutput spo = StatisticHelper.CreateCommonPoolSubtypeProportionRecord(activeAgents, iteration, (int)AgentSubtype.Co);
            spo.Subtype = EnumHelper.EnumValueAsString(AgentSubtype.Co);
            _subtypeProportionStatistic.Add(spo);
            //frequency
            _commonPoolFrequencyStatistic.Add(StatisticHelper.CreateCommonPoolFrequencyRecord(activeAgents, iteration, (int)AgentSubtype.Co));
            //params
            CommonValuesOutput valuesRecord = StatisticHelper.CreateCommonValuesRecord(activeAgents, iteration,
                VariablesUsedInCode.Endowment,
                $"AVG_{VariablesUsedInCode.AgentE}",
                $"AVG_{VariablesUsedInCode.AgentC}"
                );

            valuesRecord.Values.Add(new ValueItem { Name = "N", Value = activeAgents.Length });


            _valuesOutput.Add(valuesRecord);
        }


        private double CalculateAgentSiteWellbeing(IAgent agent)
        {
            int commonPoolC = agent.ConnectedAgents.Sum(a => a[VariablesUsedInCode.AgentC]) + agent[VariablesUsedInCode.AgentC];

            return agent[VariablesUsedInCode.AgentE] - agent[VariablesUsedInCode.AgentC]
                + agent[VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)agent.ConnectedAgents.Count + 1);
        }

        private double CalculateAgentSiteWellbeing(IAgent agent, Site centerSite)
        {
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[VariablesUsedInCode.AgentC]) + agent[VariablesUsedInCode.AgentC];

            return agent[VariablesUsedInCode.AgentE] - agent[VariablesUsedInCode.AgentC]
                + agent[VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);
        }

        private double CalculatePoolWellbeing(IAgent agent)
        {
            int commonPoolC = agent.ConnectedAgents.Sum(a => a[VariablesUsedInCode.AgentC]) + agent[VariablesUsedInCode.AgentC];

            return agent[VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)agent.ConnectedAgents.Count + 1);
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

                vacantSites.Add(currentSite);
                vacantSites.Remove(selectedSite);
            }
            else
            {
                agent[VariablesUsedInCode.AgentBetterSiteAvailable] = false;
            }
        }

        private void UpdateEndowment()
        {
            IAgent agent = _agentList.Agents.First();

            int totalE = _agentList.ActiveAgents.Sum(a => (int)a[VariablesUsedInCode.AgentE]);

            agent.SetToCommon(VariablesUsedInCode.TotalEndowment, totalE);

            //E(t) == ( r ^ p ) * ( E(t-1) – total_e(t-1) )
            int endowment = Convert.ToInt32(Math.Pow(agent[VariablesUsedInCode.R], agent[VariablesUsedInCode.P]) * (agent[VariablesUsedInCode.Endowment] - agent[VariablesUsedInCode.TotalEndowment]));

            if(endowment < 0)
            {

            }

            agent.SetToCommon(VariablesUsedInCode.Endowment, endowment);
        }

    }
}
