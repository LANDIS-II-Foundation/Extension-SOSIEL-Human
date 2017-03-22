using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Configuration
{
    public class InitialStateConfiguration
    {
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }

        public Dictionary<string, AgentStateConfiguration> AgentsState { get; set; }
    }
}
