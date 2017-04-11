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

        public IAgent[] ActiveAgents
        {
            get
            {
                return Agents.Where(a => a[VariablesUsedInCode.AgentStatus] == "active").ToArray();
            }
        }


        private AgentList() { }


        public int CalculateCommonC()
        {
            return Agents.Sum(a => a[VariablesUsedInCode.AgentC]);
        }

        public static AgentList Generate<T>(int agentNumber, Dictionary<string, T> prototypes, InitialStateConfiguration initialState, SiteList siteList = null)
            where T : class, ICloneableAgent<T>
        {
            AgentList agentList = new AgentList();

            agentList.Agents = new List<IAgent>(agentNumber);

            int ind = 1;

            initialState.AgentsState.ForEach(astate =>
            {
                T prototype = prototypes[astate.PrototypeOfAgent];

                //call before clonning agents
                prototype.AssignInitialRules(astate.AssignedRules);
                prototype.PrototypeName = astate.PrototypeOfAgent;
                prototype.InitialStateConfiguration = astate;
                prototype.SocialNetwork = initialState.SocialNetwork;

                astate.PrivateVariables.ForEach(kvp =>
                {
                    prototype[kvp.Key] = kvp.Value;
                });

                for (int i = 0; i < astate.NumberOfAgents; i++)
                {
                    T agent = prototype.Clone();

                    agent.GenerateCustomParams();

                    agent.Id = ind++;

                    agentList.Agents.Add(agent);
                }
            });


            if (initialState.SocialNetwork != SocialNetworkType.None)
            {
                InitializeSocialNetwork(initialState.SocialNetwork, agentList);
            }


            if (siteList != null)
            {
                List<Site> availableSites = siteList.AsSiteEnumerable().ToList();

                agentList.Agents.ForEach(agent =>
                {
                    Site selectedSite = availableSites.RandomizeOne();

                    selectedSite.IsOccupationChanged = true;

                    selectedSite.OccupiedBy = agent;

                    agent[VariablesUsedInCode.AgentCurrentSite] = selectedSite;

                    availableSites.Remove(selectedSite);
                });
            }

            return agentList;
        }

        private static void InitializeSocialNetwork(SocialNetworkType socialNetwork, AgentList agentList)
        {
            List<IAgent> agents = agentList.Agents.Cast<IAgent>().ToList();


            switch (socialNetwork)
            {
                case SocialNetworkType.SN1:
                    {

                        agents.ForEach(a =>
                        {
                            a.ConnectedAgents = agents.Where(a2 => a2 != a).ToList();
                        });


                        break;
                    }

                case SocialNetworkType.SN2:
                    {
                        Queue<IAgent> queue = new Queue<IAgent>(agents);

                        Queue<IAgent> leafQueue = new Queue<IAgent>();

                        int minNumberOfLeafs = 2;

                        leafQueue.Enqueue(queue.Dequeue());


                        while (queue.Count > 0)
                        {
                            IAgent vertex = leafQueue.Dequeue();

                            List<IAgent> leafs = new List<IAgent>(minNumberOfLeafs);

                            for (int i = 0; i < minNumberOfLeafs && queue.Count > 0; i++)
                            {
                                IAgent item = queue.Dequeue();

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

                case SocialNetworkType.SN3:
                    {
                        IAgent vertex = agents[0];

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
