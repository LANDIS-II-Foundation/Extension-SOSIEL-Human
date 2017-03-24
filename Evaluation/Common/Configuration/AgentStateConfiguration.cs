using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Configuration
{
    public class AgentStateConfiguration
    {
        public string[] AssignedRules { get; set; }

        public Dictionary<string, GoalStateConfiguration> GoalState { get; set; }

        public string[] MatchedRules { get; set; }

        public string[] ActivatedRules { get; set; }
    }
}
