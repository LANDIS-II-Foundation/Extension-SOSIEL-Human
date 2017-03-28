using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Configuration
{
    public class AgentStateConfiguration
    {
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }

        public string[] AssignedRules { get; set; }

        public Dictionary<string, GoalStateConfiguration> GoalState { get; set; }
    }
}
