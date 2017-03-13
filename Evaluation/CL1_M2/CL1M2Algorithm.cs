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

namespace CL1_M2
{
    using Models;

    public class CL1M2Algorithm : IAlgorithm
    {
        const string OutputFolder = @"Output\CL1_M2";

        readonly Configuration<CL1M2Agent> _configuration;

        SiteList _siteList;
        AgentList _agentList;

        List<SubtypeProportionOutput> _subtypeProportionStatistic;

        bool isAgentMovement;

        public string Name { get { return "Cognitive level 1 Model 2"; } }

        public CL1M2Algorithm(Configuration<CL1M2Agent> configuration)
        {
            _configuration = configuration;

            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);

            if (Directory.Exists(OutputFolder) == false)
                Directory.CreateDirectory(OutputFolder);
        }

        public async Task Run()
        {
            Initialize();

            await SaveState("initial");

            ExecuteAlgorithm();

            await SaveState("final");

            SaveProportionStatistic();
        }

        private void ExecuteAlgorithm()
        {
            //_subtypeProportionStatistic.Add(CalculateSubtypeProportion(0));

            for (int i = 1; i <= _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                Console.WriteLine($"Starting {i} iteration");

                isAgentMovement = false;

                List<IAgent> orderingAgents = RandomizeHelper.Randomize(_agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active"));

                List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

                foreach (var agent in orderingAgents)
                {
                    SetRandomVariables(agent);
                    CalculateParamsDependentOnSite(agent);

                    Site[] betterSites = vacantSites
                        //in this case we exclude the center site because it isn't occupied by agent
                        .Select(site => new { site, Occupied = _siteList.AdjacentSites(site).Where(s => s.IsOccupied) })
                        .Select(obj => new {
                            obj.site,
                            Wellbeing = CalculateAgentWellbeing(agent, obj.Occupied)
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
                            Site oldSite = agent[Agent.VariablesUsedInCode.AgentSite];

                            Site bestSite = betterSites.RandomizeOne();

                            agent[Agent.VariablesUsedInCode.AgentBetterSite] = bestSite;

                            rule.Apply(agent);

                            isAgentMovement = true;

                            vacantSites.Add(oldSite);
                            vacantSites.Remove(bestSite);
                        }
                    }
                }

                _subtypeProportionStatistic.Add(CalculateSubtypeProportion(i));

                if (isAgentMovement == false)
                {
                    break;
                }
            }
        }

        private double CalculateAgentWellbeing(IAgent agent, IEnumerable<Site> occupied)
        {
            return agent[Agent.VariablesUsedInCode.E] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.M] * occupied.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) / (double)occupied.Count();
        }

        private void SetRandomVariables(IAgent agent)
        {
            int agentC = LinearUniformRandom.GetInstance.Next(1, 3);

            agent[Agent.VariablesUsedInCode.E] = LinearUniformRandom.GetInstance.Next(1, agent[Agent.VariablesUsedInCode.MaxE] + 1);
            agent[Agent.VariablesUsedInCode.AgentSubtype] = (AgentSubtype)agentC;
            agent[Agent.VariablesUsedInCode.AgentC] = agent[Agent.VariablesUsedInCode.AgentSubtype] == AgentSubtype.Co ? agent[Agent.VariablesUsedInCode.E] : 0;
        }

        private void CalculateParamsDependentOnSite(IAgent agent)
        {
            Site currentSite = (Site)agent[Agent.VariablesUsedInCode.AgentSite];

            Site[] adjacentSites = _siteList.AdjacentSites(currentSite).ToArray();

            agent[Agent.VariablesUsedInCode.NeighborhoodSize] = (double)currentSite.GroupSize;
            agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);
            agent[Agent.VariablesUsedInCode.NeighborhoodUnalike] = adjacentSites.Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] != agent[Agent.VariablesUsedInCode.AgentSubtype]);

            agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);
            agent[Agent.VariablesUsedInCode.CommonPoolSize] = (double)(agent[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites]);

            agent[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion] = (agent[Agent.VariablesUsedInCode.CommonPoolSize] - agent[Agent.VariablesUsedInCode.NeighborhoodUnalike]) 
                / agent[Agent.VariablesUsedInCode.CommonPoolSize];

            //in this case we include the center site because it's occupied by agent
            agent[Agent.VariablesUsedInCode.CommonPoolC] = _siteList.AdjacentSites(currentSite, true).Where(s => s.IsOccupied).Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]);
            agent[Agent.VariablesUsedInCode.AgentSiteWellbeing] = agent[Agent.VariablesUsedInCode.E] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.M] * agent[Agent.VariablesUsedInCode.CommonPoolC] / agent[Agent.VariablesUsedInCode.CommonPoolSize];


        }


        private SubtypeProportionOutput CalculateSubtypeProportion(int iteration)
        {
            SubtypeProportionOutput sp = new SubtypeProportionOutput { Iteration = iteration };

            sp.Proportion = _siteList.AsSiteEnumerable().Average(site => _siteList.AdjacentSites(site).Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == AgentSubtype.Co) / (double)site.GroupSize);

            return sp;
        }

        //private SubtypeProportionOutput CalculateSubtypeProportion(int iteration)
        //{
        //    SubtypeProportionOutput sp = new SubtypeProportionOutput { Iteration = iteration };

        //    sp.Proportion = _siteList.AsSiteEnumerable()
        //        .Select(site => new { site, Occupied = _siteList.AdjacentSites(site, true).Where(s => s.IsOccupied) })
        //        .Average(o => o.Occupied.Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == AgentSubtype.Co) / (double)o.Occupied.Count());

        //    return sp;
        //}

        private void Initialize()
        {
            _siteList = SiteList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AlgorithmConfiguration.VacantProportion);

            _agentList = AgentList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AgentConfiguration, _siteList);
        }

        private async Task SaveState(string state)
        {
            Task nodeTask = Task.Factory.StartNew(
                () => _agentList.Agents
                .Select(a => new NodeOutput
                {
                    AgentId = a.Id,
                    Type = a[Agent.VariablesUsedInCode.AgentSubtype]
                })
                .ToArray()
            ).ContinueWith(data =>
            {
                FileHelpers.DelimitedFileEngine<NodeOutput> engine = new FileHelpers.DelimitedFileEngine<NodeOutput>();

                engine.WriteFile($@"{OutputFolder}\nodes_{state}.csv", data.Result);
            });

            Task edgeTask = Task.Factory.StartNew(
                () => _agentList.Agents
                .SelectMany(a => _siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentSite])
                .Where(s => s.IsOccupied)
                .Select(s => new EdgeOutput
                {
                    AgentId = a.Id,
                    AdjacentAgentId = s.OccupiedBy.Id
                }
                ))
                .Distinct(new EdgeOutputComparer())
                .ToArray())
            .ContinueWith(data =>
            {
                FileHelpers.DelimitedFileEngine<EdgeOutput> engine = new FileHelpers.DelimitedFileEngine<EdgeOutput>();

                engine.WriteFile($@"{OutputFolder}\edges_{state}.csv", data.Result);
            });

            await nodeTask;
            await edgeTask;
        }

        private void SaveProportionStatistic()
        {
            FileHelpers.DelimitedFileEngine<SubtypeProportionOutput> engine = new FileHelpers.DelimitedFileEngine<SubtypeProportionOutput>();

            engine.WriteFile($@"{OutputFolder}\subtype_A_proportion.csv", _subtypeProportionStatistic);
        }
    }
}
