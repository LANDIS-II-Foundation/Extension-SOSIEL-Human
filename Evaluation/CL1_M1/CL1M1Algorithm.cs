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
    public sealed class CL1M1Algorithm : AlgorithmBase, IAlgorithm
    {
        public string Name { get { return "Cognitive level 1 Model 1"; } }

        readonly Configuration<CL1M1Agent> _configuration;

        bool _isAgentMovement;

        public CL1M1Algorithm(Configuration<CL1M1Agent> configuration)
        {
            _configuration = configuration;

            _outputFolder = @"Output\CL1_M1";

            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);

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
            int agentType = (int)AgentSubtype.TypeA;

            _subtypeProportionStatistic.Add(CreateNeighborhoodSubtypeProportionRecord(0, agentType));

            for (int i = 1; i <= _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                Console.WriteLine($"Starting {i} iteration");

                _isAgentMovement = false;

                List<IAgent> orderingAgents = RandomizeHelper.Randomize(_agentList.Agents.Where(a=> a[Agent.VariablesUsedInCode.AgentStatus] == "active"));

                List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

                foreach (var agent in orderingAgents)
                {
                    CalculateParamsDependOnSite(agent);

                    Site[] betterSites = vacantSites
                        .Select(site => new {
                            Proportion = CalculateSubtypeProportion((int)agent[Agent.VariablesUsedInCode.AgentSubtype], site),
                            site
                        })
                        .Where(obj => obj.Proportion > agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion])
                        .GroupBy(obj => obj.Proportion).OrderByDescending(obj => obj.Key)
                        .Take(1).SelectMany(g => g.Select(o=>o.site)).ToArray();

                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = betterSites.Length > 0;

                    if (agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable])
                    {
                        Site currentSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];

                        Site bestSite = betterSites.Select(site => new { Distance = site.DistanceToAnother(currentSite), site }).GroupBy(o => o.Distance)
                            .OrderBy(g => g.Key).Take(1).SelectMany(g => g.Select(o=>o.site)).RandomizeOne();

                        agent[Agent.VariablesUsedInCode.AgentBetterSite] = bestSite;

                        Rule rule = agent.Rules.First();

                        if (rule.IsMatch(agent))
                        {
                            Site oldSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];

                            rule.Apply(agent);

                            _isAgentMovement = true;

                            vacantSites.Add(oldSite);
                            vacantSites.Remove(bestSite);
                        }
                    }
                }

                _subtypeProportionStatistic.Add(CreateNeighborhoodSubtypeProportionRecord(i, agentType));

                if(_isAgentMovement == false)
                {
                    break;
                }
            }
        }

        private void CalculateParamsDependOnSite(IAgent agent)
        {
            Site currentSite = (Site)agent[Agent.VariablesUsedInCode.AgentCurrentSite];

            Site[] adjacentSites = _siteList.AdjacentSites(currentSite).ToArray();

            agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion] = CalculateSubtypeProportion((int)agent[Agent.VariablesUsedInCode.AgentSubtype], currentSite);

            //optional calculations, they may be use in rules

            agent[Agent.VariablesUsedInCode.NeighborhoodSize] = (double)currentSite.GroupSize;

            agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);

            agent[Agent.VariablesUsedInCode.NeighborhoodUnalike] = adjacentSites.Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] != agent[Agent.VariablesUsedInCode.AgentSubtype]);
        }


        protected override double CalculateSubtypeProportion(int subtype, Site centerSite)
        {
            return _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied)
                .Count(s => (int)s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == subtype) / (double)centerSite.GroupSize;
        }
    }
}
