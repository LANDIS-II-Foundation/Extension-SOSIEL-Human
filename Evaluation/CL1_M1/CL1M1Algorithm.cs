using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Configuration;
using Common.Algorithm;
using Common.Entities;
using Common.Helpers;

namespace CL1_M1
{
    using Models;



    public class CL1M1Algorithm : IAlgorithm
    {
        readonly Configuration<CL1M1Agent> _configuration;

        SiteList _siteList;
        AgentList _agentList;

        public string Name { get { return "Cognitive level 1 Model 1"; } }

        public CL1M1Algorithm(Configuration<CL1M1Agent> configuration)
        {
            _configuration = configuration;
        }

        public async void Run()
        {
            await Initialize();

            ExecuteAlgorithm();

        }

        private void ExecuteAlgorithm()
        {
            for (int i = 1; i <= _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                List<IAgent> orderingAgents = RandomizeHelper.Randomize(_agentList.Agents);

                List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

                foreach (var agent in orderingAgents)
                {
                    Site currentSite = agent[Agent.VariablesUsedInCode.AgentSite];

                    CalculateParamsDependentOnSite(agent);

                    Site[] betterSites = vacantSites.Where(
                        s => (_siteList.AdjacentSites(s)
                                .Where(adjs => adjs.IsOccupied)
                                .Count(adjs => adjs.OccupiedBy.Variables[Agent.VariablesUsedInCode.AgentSubtype] == agent[Agent.VariablesUsedInCode.AgentSubtype]))
                            > agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion]).ToArray();

                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = betterSites.Length > 0;

                    //DistanceComparer distComparer = new DistanceComparer(agent[Agent.VariablesUsedInCode.AgentSite]);

                    if (agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable])
                    {
                        Site bestSite = betterSites.Select(site => new { Distance = site.DistanceToAnother(currentSite), site }).GroupBy(o => o.Distance)
                            .OrderBy(g => g.Key).First().Select(g => g.site).RandomizeOne();

                        Rule rule = agent.Rules.First();

                        if (rule.IsMatch(agent))
                        {
                            Site oldSite = agent[Agent.VariablesUsedInCode.AgentSite];

                            oldSite.OccupiedBy = null;

                            agent[Agent.VariablesUsedInCode.AgentBetterSite] = bestSite;

                            bestSite.OccupiedBy = agent;

                            rule.Apply(agent);

                            vacantSites.Add(oldSite);
                            vacantSites.Remove(bestSite);
                        }
                    }
                }
            }
        }

        private void CalculateParamsDependentOnSite(IAgent agent)
        {
            Site currentSite = (Site)agent[Agent.VariablesUsedInCode.AgentSite];

            Site[] adjacentSites = _siteList.AdjacentSites(currentSite).ToArray();

            agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = adjacentSites.Count(s => s.IsOccupied == false);

            agent[Agent.VariablesUsedInCode.NeighborhoodUnalike] = adjacentSites.Where(s => s.IsOccupied)
                .Count(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] != agent[Agent.VariablesUsedInCode.AgentSubtype]);

            agent[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion] = currentSite.GroupSize -
                (agent[Agent.VariablesUsedInCode.NeighborhoodVacantSites] + agent[Agent.VariablesUsedInCode.NeighborhoodUnalike]);
        }


        private async Task Initialize()
        {
            _siteList = SiteList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AlgorithmConfiguration.VacantProportion);

            _agentList = AgentList.Generate(_configuration.AlgorithmConfiguration.AgentCount,
                _configuration.AgentConfiguration, _siteList);

            await SaveState("initial");
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

                engine.WriteFile($"nodes_{state}.csv", data.Result);
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
                .ToArray())
            .ContinueWith(data =>
            {
                FileHelpers.DelimitedFileEngine<EdgeOutput> engine = new FileHelpers.DelimitedFileEngine<EdgeOutput>();

                engine.WriteFile($"edges_{state}.csv", data.Result);
            });

            await nodeTask;
            await edgeTask;
        }
    }
}
