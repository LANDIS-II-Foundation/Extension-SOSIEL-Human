using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Configuration;
    using Randoms;
    using Environments;

    public class AgentList
    {
        public List<IAgent> Agents { get; set; }


        private AgentList() { }







        public int CalculateCommonC()
        {
            return Agents.Sum(a => a[Agent.VariablesUsedInCode.AgentC]);
        }



        public static AgentList Generate<T>(int agentNumber, T prototype, SiteList siteList) where T : class, ICloneableAgent<T>
        {
            List<Site> availableSites = siteList.AsSiteEnumerable().ToList();

            AgentList agentList = new AgentList();

            agentList.Agents = new List<IAgent>(agentNumber);

            for (int i = 1; i <= agentNumber; i++)
            {
                T agent = prototype.Clone();

                agent.GenerateCustomParams();

                agent.Id = i;

                Site selectedSite = availableSites[LinearUniformRandom.GetInstance.Next(availableSites.Count)];

                selectedSite.OccupiedBy = agent;

                agent[Agent.VariablesUsedInCode.AgentCurrentSite] = selectedSite;

                agentList.Agents.Add(agent);
                availableSites.Remove(selectedSite);
            }

            return agentList;
        }


        public static AgentList Generate2<T>(int agentNumber, T prototype, InitialStateConfiguration initialState) where T: class, IAgent, IAnticipatedInfluenceState
        {
            AgentList agentList = new AgentList();

            agentList.Agents = new List<IAgent>(agentNumber);

            //for (int i = 1; i <= agentNumber; i++)
            //{
            //    T agent = prototype.Clone();

            //    agent.GenerateCustomParams();

            //    agent.Id = i;

            //    Site selectedSite = availableSites[LinearUniformRandom.GetInstance.Next(availableSites.Count)];

            //    selectedSite.OccupiedBy = agent;

            //    agent[Agent.VariablesUsedInCode.AgentCurrentSite] = selectedSite;

            //    agentList.Agents.Add(agent);
            //    availableSites.Remove(selectedSite);
            //}

            return agentList;
        }
    }
}
