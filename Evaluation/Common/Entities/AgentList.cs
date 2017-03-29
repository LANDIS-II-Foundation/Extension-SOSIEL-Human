using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Configuration;
    using Randoms;
    using Helpers;

    public class AgentList
    {
        public List<IAgent> Agents { get; set; }


        private AgentList() { }


        public int CalculateCommonC()
        {
            return Agents.Sum(a => a[Agent.VariablesUsedInCode.AgentC]);
        }

        public static AgentList Generate<T>(int agentNumber, T prototype, SiteList siteList) where T : class, IAgent, ICloneableAgent<T>
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


        public static AgentList Generate2<T>(int agentNumber, Dictionary<string,T> prototypes, InitialStateConfiguration initialState) where T: class, IAgent, ICloneableAgent<T>, IConfigurableAgent
        {
            AgentList agentList = new AgentList();

            agentList.Agents = new List<IAgent>(agentNumber);

            if (agentNumber != initialState.AgentsState.Sum(astate => astate.NumberOfAgents))
                throw new Exception($"Number of agents which described in algorithm.json and sum of agets in {typeof(T).Name}.configuration.json are different");

            int ind = 1;

            initialState.AgentsState.ForEach(astate =>
            {
                for (int i = 0; i < astate.NumberOfAgents; i++)
                {
                    T agent = prototypes[astate.PrototypeOfAgent].Clone();

                    agent.GenerateCustomParams();

                    agent.Id = ind++;

                    agent.AssignRules(astate.AssignedRules);

                    agent.InitialState = astate;

                    agentList.Agents.Add(agent);
                }
            });

            return agentList;
        }
    }
}
