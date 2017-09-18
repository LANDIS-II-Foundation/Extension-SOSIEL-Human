using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Configuration;
    using Exceptions;
    using Randoms;
    using Helpers;
    using Enums;


    public class AgentList
    {
        public List<IAgent> Agents { get; private set; }

        public List<AgentPrototype> Prototypes { get; private set; }

        public IAgent[] ActiveAgents
        {
            get
            {
                return Agents.Where(a => a[SosielVariables.AgentStatus] == true).ToArray();
            }
        }

        public AgentList()
        {
            Agents = new List<IAgent>();
            Prototypes = new List<AgentPrototype>();
        }


        /// <summary>
        /// Searches for prototypes with following prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<AgentPrototype> GetPrototypesWithPrefix(string prefix)
        {
            return Prototypes.Where(prototype => prototype.NamePrefix == prefix);
        }

        /// <summary>
        /// Searches for agents with following prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgentsWithPrefix(string prefix)
        {
            return ActiveAgents.Where(agent => agent.Prototype.NamePrefix == prefix);
        }
    }
}
