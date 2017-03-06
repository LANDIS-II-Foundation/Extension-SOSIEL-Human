using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Common.Configuration
{
    using Enums;

    [JsonObject]
    public class AlgorithmConfiguration
    {
        public Model Model { get;  set; }

        public CognitiveLevel CognitiveLevel { get;  set; }

        public ActionTaking ActionTaking { get;  set; }

        public int IterationCount { get;  set; }

        public int AgentCount { get;  set; }

        public double VacantProportion { get;  set; }
    }
}
