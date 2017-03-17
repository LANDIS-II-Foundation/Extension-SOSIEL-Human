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
    public sealed class CL1M2Algorithm : AlgorithmBase, IAlgorithm
    {
        public string Name { get { return "Cognitive level 1 Model 2"; } }

        readonly Configuration<CL1M2Agent> _configuration;

        List<CommonPoolOutput> _commonPoolStatistic;

        bool _isAgentMovement;

        public CL1M2Algorithm(Configuration<CL1M2Agent> configuration)
        {
            _configuration = configuration;

            _outputFolder = @"Output\CL1_M2";

            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _commonPoolStatistic = new List<CommonPoolOutput>(_configuration.AlgorithmConfiguration.IterationCount);

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }

        protected override void Initialize()
        {
            _siteList = SiteList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AlgorithmConfiguration.VacantProportion);

            _agentList = AgentList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AgentConfiguration, _siteList);
        }

        protected override void ExecuteAlgorithm()
        {
            _subtypeProportionStatistic.Add(CreateSubtypeProportionRecord(0));

            for (int i = 1; i <= _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                Console.WriteLine($"Starting {i} iteration");

                _isAgentMovement = false;

                List<IAgent> orderingAgents = RandomizeHelper.Randomize(_agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active"));

                List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

                foreach (var agent in orderingAgents)
                {
                    CalculateParamsDependOnSite(agent);

                    Site[] betterSites = vacantSites
                        .Select(site => new
                        {
                            site,
                            Wellbeing = CalculateAgentWellbeing(agent, site)
                        })
                        .Where(obj => obj.Wellbeing > agent[Agent.VariablesUsedInCode.AgentSiteWellbeing])
                        .GroupBy(obj => obj.Wellbeing).OrderByDescending(obj => obj.Key)
                        .Take(1).SelectMany(g => g.Select(o => o.site)).ToArray();

                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = betterSites.Length > 0;

                    if (agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable])
                    {
                        Rule rule = agent.Rules.First();

                        if (rule.IsMatch(agent))
                        {
                            Site oldSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];

                            Site bestSite = betterSites.RandomizeOne();

                            agent[Agent.VariablesUsedInCode.AgentBetterSite] = bestSite;

                            rule.Apply(agent);

                            _isAgentMovement = true;

                            vacantSites.Add(oldSite);
                            vacantSites.Remove(bestSite);
                        }
                    }
                }

                _subtypeProportionStatistic.Add(CreateSubtypeProportionRecord(i));
                _commonPoolStatistic.Add(CreateCommonPoolStatisticRecord(i));

                if (_isAgentMovement == false)
                {
                    break;
                }
            }
        }

        protected override void SaveCustomStatistic()
        {
            SaveCommonPoolStatistic();
        }

        private void CalculateParamsDependOnSite(IAgent agent)
        {
            Site currentSite = (Site)agent[Agent.VariablesUsedInCode.AgentCurrentSite];

            Site[] adjacentSites = _siteList.CommonPool(currentSite).ToArray();

            agent[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = CalculateSubtypeProportion(agent[Agent.VariablesUsedInCode.AgentSubtype], currentSite);

            agent[Agent.VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentWellbeing(agent, currentSite);


            //optional calculations, they may be use in rules

            agent[Agent.VariablesUsedInCode.NeighborhoodSize] = (double)currentSite.GroupSize;
            agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);
            agent[Agent.VariablesUsedInCode.NeighborhoodUnalike] = adjacentSites.Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] != agent[Agent.VariablesUsedInCode.AgentSubtype]);

            agent[Agent.VariablesUsedInCode.CommonPoolSize] = agent[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites];
            agent[Agent.VariablesUsedInCode.CommonPoolC] = _siteList.CommonPool(currentSite).Where(s => s.IsOccupied).Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]);

            agent[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = CalculateSubtypeProportion(agent[Agent.VariablesUsedInCode.AgentSubtype], currentSite);
        }

        private double CalculateAgentWellbeing(IAgent agent, Site centerSite)
        {
            //we take only adjacement sites because in some cases center site can be empty
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s=>s.IsOccupied).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.Engage] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);
        }

        //private double CalculateCommonPoolWellbeing(Site centerSite)
        //{

        //}

        private CommonPoolOutput CreateCommonPoolStatisticRecord(int iteration)
        {
            CommonPoolOutput cp = new CommonPoolOutput { Iteration = iteration };

            cp.CommonPools = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied)
                .Select(site => CalculateCommonPoolStat(site)).ToArray();

            return cp;
        }

        private CommonPoolStat CalculateCommonPoolStat(Site centerSite)
        {
            IAgent agent = centerSite.OccupiedBy;
            var occupiedCommonPool = _siteList.CommonPool(centerSite).Where(s => s.IsOccupied).ToArray();
            int commonPoolC = occupiedCommonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]);
            double poolSize = occupiedCommonPool.Length;


            return new CommonPoolStat
            {
                Center = new CommonPoolCenter { X = centerSite.HorizontalPosition + 1, Y = centerSite.VerticalPosition + 1 },  //zero-based numeration
                CommonPoolWellbeing = agent[Agent.VariablesUsedInCode.Engage] * poolSize + commonPoolC
                    * (agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] / poolSize - 1)//,
                //CoProportion = CalculateSubtypeProportion(AgentSubtype.Co, occupiedCommonPool)
            };
        }
        private double CalculateSubtypeProportion(AgentSubtype subtype, Site centerSite)
        {
            var occupiedCommonPool = _siteList.CommonPool(centerSite).Where(s => s.IsOccupied).ToArray();

            return CalculateSubtypeProportion(subtype, occupiedCommonPool);
        }
        
        private double CalculateSubtypeProportion(AgentSubtype subtype, Site[] occupiedCommonPool)
        {
            return occupiedCommonPool.Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == subtype)
                / (double)occupiedCommonPool.Length;
        }

        private SubtypeProportionOutput CreateSubtypeProportionRecord(int iteration)
        {
            SubtypeProportionOutput sp = new SubtypeProportionOutput { Iteration = iteration };

            sp.Proportion = _siteList.AsSiteEnumerable().Where(s=>s.IsOccupied && s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == AgentSubtype.Co)
                .Average(site => CalculateSubtypeProportion(AgentSubtype.Co, site));

            return sp;
        }

        private AvgWellbeingOutput CreateAvgWellbeingStatisticRecord(int iteration)
        {
            AvgWellbeingOutput aw = new AvgWellbeingOutput { Iteration = iteration };

            aw.Avgs = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active")
                .GroupBy(a => (AgentSubtype)a[Agent.VariablesUsedInCode.AgentSubtype])
                .OrderBy(g => g.Key)
                .Select(g => new AvgWellbeingItem { Type = EnumHelper.EnumValueAsString(g.Key), AvgValue = g.Average(a => (double)CalculateAgentWellbeing(a, a[Agent.VariablesUsedInCode.AgentCurrentSite])) }).ToArray();

            return aw;
        }

        private void SaveCommonPoolStatistic()
        {
            ResultSavingHelper.Save(_commonPoolStatistic.Select(cps => new SimpleLineOutput(cps)), $@"{_outputFolder}\common_pool_statistic.csv");
        }
    }
}
