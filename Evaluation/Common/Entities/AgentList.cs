using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Randoms;
    using Environment;

    public class AgentList
    {
        public List<Agent> Agents { get; set; }


        private AgentList() { }

        public static AgentList Generate(int agentCount, Agent prototype, SiteList siteList)
        {
            List<Site> availableSites = new List<Site>(siteList.Sites);

            AgentList agentList = new AgentList();

            agentList.Agents = new List<Agent>(agentCount);

            for (int i = 1; i <= agentCount; i++)
            {
                Agent agent = prototype.Clone();

                agent.Id = i;

                agentList.Agents.Add(agent);

                Site selectedSite = availableSites[LinearUniformRandom.GetInstance.Next(availableSites.Count)];

                selectedSite.OccupiedBy = agent;

                agent[Agent.UsingInCodeVariables.AgentSite] = selectedSite;

                availableSites.Remove(selectedSite);
            }

            return agentList;
        }
    }
}
