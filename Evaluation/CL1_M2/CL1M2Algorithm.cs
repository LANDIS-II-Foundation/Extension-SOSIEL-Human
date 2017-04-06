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

        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                RuleSelectionEnabled = true,
                AgentRandomizationEnabled = true
            };
        }

        public CL1M2Algorithm(Configuration<CL1M2Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _averageWellbeing = new List<AvgWellbeingOutput>(_configuration.AlgorithmConfiguration.IterationCount);

            _outputFolder = @"Output\CL1_M2";

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
        }

        protected override void AfterAlgorithmExecuted()
        {
            StatisticHelper.SaveState(_outputFolder, "final", _agentList.ActiveAgents, _siteList);

            StatisticHelper.Save(_subtypeProportionStatistic, $@"{_outputFolder}\subtype_proportion_statistic.csv");
            StatisticHelper.Save(_averageWellbeing, $@"{_outputFolder}\subtype_wellbeing_statistic.csv");
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

            });
        }

        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;
            IAgent agent = activeAgents.First();


            _subtypeProportionStatistic.Add(StatisticHelper.CreateSubtypeProportionRecord(activeAgents, iteration, (int)AgentSubtype.Co));
            _averageWellbeing.Add(StatisticHelper.CreateAvgWellbeingStatisticRecord(activeAgents, iteration));
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
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];


            double wellbeing = agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);

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
                };
            });
        }
    }


    //public sealed class CL1M2Algorithm : AlgorithmBase, IAlgorithm
    //{
    //    public string Name { get { return "Cognitive level 1 Model 2"; } }

    //    readonly Configuration<CL1M2Agent> _configuration;

    //    List<AvgWellbeingOutput> _avgWellbeing;
    //    //List<CommonPoolOutput> _commonPoolStatistic;

    //    bool _isAgentMovement;

    //    public CL1M2Algorithm(Configuration<CL1M2Agent> configuration)
    //    {
    //        _configuration = configuration;

    //        _outputFolder = @"Output\CL1_M2";

    //        _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);
    //        _avgWellbeing = new List<AvgWellbeingOutput>(_configuration.AlgorithmConfiguration.IterationCount);
    //        //_commonPoolStatistic = new List<CommonPoolOutput>(_configuration.AlgorithmConfiguration.IterationCount);

    //        if (Directory.Exists(_outputFolder) == false)
    //            Directory.CreateDirectory(_outputFolder);
    //    }

    //    protected override void Initialize()
    //    {
    //        _siteList = SiteList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
    //            _configuration.AlgorithmConfiguration.VacantProportion);

    //        _agentList = AgentList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
    //            _configuration.AgentConfiguration, _siteList);
    //    }

    //    protected override void ExecuteAlgorithm()
    //    {
    //        int agentType = (int)AgentSubtype.Co;


    //        _subtypeProportionStatistic.Add(CreateCommonPoolSubtypeProportionRecord(0, agentType));
    //        _avgWellbeing.Add(CreateAvgSubtypeWellbeingStatisticRecord(0));

    //        for (int i = 1; i <= _configuration.AlgorithmConfiguration.IterationCount; i++)
    //        {
    //            Console.WriteLine($"Starting {i} iteration");

    //            _isAgentMovement = false;

    //            List<IAgent> orderingAgents = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active").Randomize().ToList();

    //            List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

    //            foreach (var agent in orderingAgents)
    //            {
    //                CalculateParamsDependOnSite(agent);

    //                Site[] betterSites = vacantSites.AsParallel()
    //                    .Select(site => new
    //                    {
    //                        site,
    //                        Wellbeing = CalculateAgentWellbeing(agent, site)
    //                    })
    //                    .Where(obj => obj.Wellbeing > agent[Agent.VariablesUsedInCode.AgentSiteWellbeing]).AsSequential()
    //                    .GroupBy(obj => obj.Wellbeing).OrderByDescending(obj => obj.Key)
    //                    .Take(1).SelectMany(g => g.Select(o => o.site)).ToArray();

    //                agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = betterSites.Length > 0;

    //                if (agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable])
    //                {
    //                    Rule rule = agent.Rules.First();

    //                    if (rule.IsMatch(agent))
    //                    {
    //                        Site oldSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];

    //                        Site bestSite = betterSites.RandomizeOne();

    //                        agent[Agent.VariablesUsedInCode.AgentBetterSite] = bestSite;

    //                        rule.Apply(agent);

    //                        _isAgentMovement = true;

    //                        vacantSites.Add(oldSite);
    //                        vacantSites.Remove(bestSite);
    //                    }
    //                }
    //            }

    //            _subtypeProportionStatistic.Add(CreateCommonPoolSubtypeProportionRecord(i, agentType));
    //            _avgWellbeing.Add(CreateAvgSubtypeWellbeingStatisticRecord(i));
    //            //_commonPoolStatistic.Add(CreateCommonPoolStatisticRecord(i));

    //            if (_isAgentMovement == false)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    protected override void SaveCustomStatistic()
    //    {
    //        //SaveCommonPoolStatistic();
    //        SaveAvgWellbeingStatistic();
    //    }

    //    private void CalculateParamsDependOnSite(IAgent agent)
    //    {
    //        Site currentSite = (Site)agent[Agent.VariablesUsedInCode.AgentCurrentSite];

    //        Site[] adjacentSites = _siteList.CommonPool(currentSite).ToArray();

    //        agent[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = CalculateSubtypeProportion((int)agent[Agent.VariablesUsedInCode.AgentSubtype], currentSite);

    //        agent[Agent.VariablesUsedInCode.AgentSiteWellbeing] = CalculateAgentWellbeing(agent, currentSite);


    //        //optional calculations, they may be use in rules

    //        agent[Agent.VariablesUsedInCode.NeighborhoodSize] = (double)currentSite.GroupSize;
    //        agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);
    //        agent[Agent.VariablesUsedInCode.NeighborhoodUnalike] = adjacentSites.Where(s => s.IsOccupied)
    //            .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] != agent[Agent.VariablesUsedInCode.AgentSubtype]);

    //        agent[Agent.VariablesUsedInCode.CommonPoolSize] = agent[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites];
    //        agent[Agent.VariablesUsedInCode.CommonPoolC] = _siteList.CommonPool(currentSite).Where(s => s.IsOccupied).Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]);

    //        agent[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = CalculateSubtypeProportion((int)agent[Agent.VariablesUsedInCode.AgentSubtype], currentSite);
    //    }

    //    protected override double CalculateAgentWellbeing(IAgent agent, Site centerSite)
    //    {
    //        //we take only adjacement sites because in some cases center site can be empty
    //        var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied).ToArray();

    //        int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

    //        return agent[Agent.VariablesUsedInCode.Engage] - agent[Agent.VariablesUsedInCode.AgentC]
    //            + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);
    //    }

    //    //private CommonPoolOutput CreateCommonPoolStatisticRecord(int iteration)
    //    //{
    //    //    CommonPoolOutput cp = new CommonPoolOutput { Iteration = iteration };

    //    //    cp.CommonPools = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied).AsParallel().AsOrdered()
    //    //        .Select(site => CalculateCommonPoolStat(site)).ToArray();

    //    //    return cp;
    //    //}

    //    //private CommonPoolStat CalculateCommonPoolStat(Site centerSite)
    //    //{
    //    //    IAgent agent = centerSite.OccupiedBy;
    //    //    var occupiedCommonPool = _siteList.CommonPool(centerSite).Where(s => s.IsOccupied).ToArray();
    //    //    int commonPoolC = occupiedCommonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]);
    //    //    double poolSize = occupiedCommonPool.Length;


    //    //    return new CommonPoolStat
    //    //    {
    //    //        Center = new CommonPoolCenter { X = centerSite.HorizontalPosition + 1, Y = centerSite.VerticalPosition + 1 },  //zero-based numeration
    //    //        CommonPoolWellbeing = agent[Agent.VariablesUsedInCode.Engage] * poolSize + commonPoolC
    //    //            * (agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] / poolSize - 1)//,
    //    //        //CoProportion = CalculateSubtypeProportion(AgentSubtype.Co, occupiedCommonPool)
    //    //    };
    //    //}




    //    //private double CalculateSubtypeProportion(AgentSubtype subtype, Site[] occupiedCommonPool)
    //    //{
    //    //    return occupiedCommonPool.Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == subtype)
    //    //        / (double)occupiedCommonPool.Length;
    //    //}



    //    private AvgWellbeingOutput CreateAvgSubtypeWellbeingStatisticRecord(int iteration)
    //    {
    //        AvgWellbeingOutput aw = new AvgWellbeingOutput { Iteration = iteration };

    //        aw.Avgs = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active")
    //            .GroupBy(a => (AgentSubtype)a[Agent.VariablesUsedInCode.AgentSubtype])
    //            .OrderBy(g => g.Key)
    //            .Select(g => new AvgWellbeingItem { Type = EnumHelper.EnumValueAsString(g.Key), AvgValue = g.Average(a => (double)CalculateAgentWellbeing(a, a[Agent.VariablesUsedInCode.AgentCurrentSite])) }).ToArray();

    //        return aw;
    //    }

    //    //private void SaveCommonPoolStatistic()
    //    //{
    //    //    ResultSavingHelper.Save(_commonPoolStatistic.Select(cps => new SimpleLineOutput(cps)), $@"{_outputFolder}\common_pool_statistic.csv");
    //    //}

    //    private void SaveAvgWellbeingStatistic()
    //    {
    //        ResultSavingHelper.Save(_avgWellbeing, $@"{_outputFolder}\subtype_wellbeing_statistic.csv");
    //    }
    //}
}
