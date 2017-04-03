using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Configuration;
    using Randoms;
    using Helpers;
    using Enums;


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


        public static AgentList Generate2<T>(int agentNumber, Dictionary<string, T> prototypes, InitialStateConfiguration initialState, SiteList siteList) where T : class, IConfigurableAgent, ICloneableAgent<T>
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


            if (initialState.SocialNetwork != SocialNetworkTypes.None)
            {
                InitializeSocialNetwork(initialState.SocialNetwork, agentList);
            }


            if (siteList != null)
            {
                List<Site> availableSites = siteList.AsSiteEnumerable().ToList();

                agentList.Agents.ForEach(agent =>
                {
                    Site selectedSite = availableSites.RandomizeOne();

                    selectedSite.OccupiedBy = agent;

                    agent[Agent.VariablesUsedInCode.AgentCurrentSite] = selectedSite;

                    availableSites.Remove(selectedSite);
                });
            }

            return agentList;
        }

        private static void InitializeSocialNetwork(SocialNetworkTypes socialNetwork, AgentList agentList)
        {
            List<IConfigurableAgent> agents = agentList.Agents.Cast<IConfigurableAgent>().ToList();


            switch (socialNetwork)
            {
                case SocialNetworkTypes.SN1:
                    {
                        
                        agents.ForEach(a =>
                        {
                            a.ConnectedAgents = agents.Where(a2=>a2 != a).ToList();
                        });


                        break;
                    }

                case SocialNetworkTypes.SN2:
                    {
                        Queue<IConfigurableAgent> queue = new Queue<IConfigurableAgent>(agents);

                        Queue<IConfigurableAgent> leafQueue = new Queue<IConfigurableAgent>();

                        int minNumberOfLeafs = 2;

                        leafQueue.Enqueue(queue.Dequeue());


                        while (queue.Count > 0)
                        {
                            IConfigurableAgent vertex = leafQueue.Dequeue();

                            List<IConfigurableAgent> leafs = new List<IConfigurableAgent>(minNumberOfLeafs);

                            for (int i = 0; i < minNumberOfLeafs && queue.Count > 0; i++)
                            {
                                IConfigurableAgent item = queue.Dequeue();

                                leafs.Add(item);
                                leafQueue.Enqueue(item);
                            }

                            if (queue.Count < minNumberOfLeafs)
                            {
                                leafs.AddRange(queue);
                                queue.Clear();
                            }


                            vertex.ConnectedAgents.AddRange(leafs);
                        }

                        break;
                    }

                case SocialNetworkTypes.SN3:
                    {
                        IConfigurableAgent vertex = agents[0];

                        agents.Remove(vertex);


                        vertex.ConnectedAgents.AddRange(agents);

                        agents.ForEach(a =>
                        {
                            a.ConnectedAgents.Add(vertex);

                        });


                        break;
                    }
            }
        }

    }
}
