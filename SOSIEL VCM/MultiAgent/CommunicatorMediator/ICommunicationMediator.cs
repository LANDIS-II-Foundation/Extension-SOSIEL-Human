using System;
using System.Collections.Generic;
using MultiAgent;

namespace CommunicatorMediator
{
    public interface ICommunicationMediator<T>
    {
        List<Dictionary<Guid, AgentBase<T>>> AgentList { get; set; }

        void Register(Dictionary<Guid, AgentBase<T>> agentsToRegister);

        void Notify(AgentBase<T> agentKey);
    }
}