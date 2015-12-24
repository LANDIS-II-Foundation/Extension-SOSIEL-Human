using System;
using System.Collections.Generic;
using System.Linq;
using MultiAgent;

namespace CommunicatorMediator
{
    /// <summary>
    /// Simple implementation of Communicator service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommunicationMediator<T> : ICommunicationMediator<T>
    {
        public List<Dictionary<Guid, AgentBase<T>>> AgentList { get; set; }

        public void Register(Dictionary<Guid, AgentBase<T>> agentsToRegister)
        {
            AgentList.Add(agentsToRegister);
        }

        public void Notify(AgentBase<T> agent)
        {
            var agentsList = AgentList.Where(x => x.ContainsKey(agent.Identifier)).Select(x=>x.Values);

            foreach (var agents in agentsList)
            {
               
            }
        }
    }
}