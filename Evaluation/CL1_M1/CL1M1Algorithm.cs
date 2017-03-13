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

namespace CL1_M1
{
    using Models;


    public class CL1M1Algorithm : IAlgorithm
    {
        const string OutputFolder = @"Output\CL1_M1";

        readonly Configuration<CL1M1Agent> _configuration;

        SiteList _siteList;
        AgentList _agentList;

        List<SubtypeProportionOutput> _subtypeProportionStatistic;

        bool isAgentMovement;

        public string Name { get { return "Cognitive level 1 Model 1"; } }

        public CL1M1Algorithm(Configuration<CL1M1Agent> configuration)
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
            _subtypeProportionStatistic.Add(CalculateSubtypeProportion(0));

            for (int i = 1; i <= _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                Console.WriteLine($"Starting {i} iteration");

                isAgentMovement = false;

                List<IAgent> orderingAgents = RandomizeHelper.Randomize(_agentList.Agents.Where(a=> a[Agent.VariablesUsedInCode.AgentStatus] == "active"));

                List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

                foreach (var agent in orderingAgents)
                {
                    CalculateParamsDependentOnSite(agent);

                    Site[] betterSites = vacantSites
                        .Select(site => new {
                            Proportion = _siteList.AdjacentSites(site).Where(s => s.IsOccupied)
                                    .Count(s => s.OccupiedBy.Variables[Agent.VariablesUsedInCode.AgentSubtype] == agent[Agent.VariablesUsedInCode.AgentSubtype])
                                    / (double)site.GroupSize,
                            site
                        })
                        .Where(obj => obj.Proportion > agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion])
                        .GroupBy(obj => obj.Proportion).OrderByDescending(obj => obj.Key)
                        .Take(1).SelectMany(g => g.Select(o=>o.site)).ToArray();

                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = betterSites.Length > 0;

                    if (agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable])
                    {
                        Site currentSite = agent[Agent.VariablesUsedInCode.AgentSite];

                        

                        Site bestSite = betterSites.Select(site => new { Distance = site.DistanceToAnother(currentSite), site }).GroupBy(o => o.Distance)
                            .OrderBy(g => g.Key).Take(1).SelectMany(g => g.Select(o=>o.site)).RandomizeOne();

                        agent[Agent.VariablesUsedInCode.AgentBetterSite] = bestSite;

                        Rule rule = agent.Rules.First();

                        if (rule.IsMatch(agent))
                        {
                            var temp = _siteList.AdjacentSites(currentSite);

                            isAgentMovement = true;

                            Site oldSite = agent[Agent.VariablesUsedInCode.AgentSite];

                            rule.Apply(agent);

                            vacantSites.Add(oldSite);
                            vacantSites.Remove(bestSite);
                        }
                    }
                }

                _subtypeProportionStatistic.Add(CalculateSubtypeProportion(i));

                if(isAgentMovement == false)
                {
                    break;
                }
            }
        }

        private void CalculateParamsDependentOnSite(IAgent agent)
        {
            Site currentSite = (Site)agent[Agent.VariablesUsedInCode.AgentSite];

            Site[] adjacentSites = _siteList.AdjacentSites(currentSite).ToArray();

            agent[Agent.VariablesUsedInCode.NeighborhoodSize] = (double)currentSite.GroupSize;

            agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion] = (adjacentSites.Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == agent[Agent.VariablesUsedInCode.AgentSubtype])) / agent[Agent.VariablesUsedInCode.NeighborhoodSize];

            //optional calculations

            agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);

            agent[Agent.VariablesUsedInCode.NeighborhoodUnalike] = adjacentSites.Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] != agent[Agent.VariablesUsedInCode.AgentSubtype]);
        }

        private SubtypeProportionOutput CalculateSubtypeProportion(int iteration)
        {
            SubtypeProportionOutput sp = new SubtypeProportionOutput { Iteration = iteration };

            sp.Proportion = _siteList.AsSiteEnumerable().Average(site => _siteList.AdjacentSites(site).Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == AgentSubtype.TypeA) / (double)site.GroupSize);

            return sp;
        }

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
                    Type = a.Variables[Agent.VariablesUsedInCode.AgentSubtype]
                })
                .ToArray()
            ).ContinueWith(data =>
            {
                FileHelpers.DelimitedFileEngine<NodeOutput> engine = new FileHelpers.DelimitedFileEngine<NodeOutput>();

                engine.WriteFile($@"{OutputFolder}\nodes_{state}.csv", data.Result);
            });

            Task edgeTask = Task.Factory.StartNew(
                () => _agentList.Agents
                .SelectMany(a => _siteList.AdjacentSites((Site)a.Variables[Agent.VariablesUsedInCode.AgentSite])
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
