using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHuman.Configuration
{
    using Enums;

    public class InitialStateConfiguration
    {
        public bool GenerateGoalImportance { get; set; }

        public bool RandomlySelectRule { get; set; }

        [JsonRequired]
        public AgentStateConfiguration[] AgentsState { get; set; }
    }
}
