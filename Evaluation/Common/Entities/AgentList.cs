using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Randoms;
    using Environment;

    public class AgentList
    {
        public List<IAgent> Agents { get; set; }


        private AgentList() { }

        public static AgentList Generate<T>(int agentCount, T prototype, SiteList siteList) where T : class, ICloneableAgent<T>
        {
            List<Site> availableSites = siteList.AsSiteEnumerable().ToList();

            AgentList agentList = new AgentList();

            agentList.Agents = new List<IAgent>(agentCount);

            for (int i = 1; i <= agentCount; i++)
            {
                T agent = prototype.Clone();

                agent.GenerateCustomParams();

                agent.Id = i;

                Site selectedSite = availableSites[LinearUniformRandom.GetInstance.Next(availableSites.Count)];

                selectedSite.OccupiedBy = agent;

                agent[Agent.VariablesUsedInCode.AgentSite] = selectedSite;

                agentList.Agents.Add(agent);
                availableSites.Remove(selectedSite);
            }

            return agentList;
        }
    }
}
