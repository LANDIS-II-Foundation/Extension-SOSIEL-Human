using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHuman.Configuration
{
    public class GoalStateConfiguration
    {
        [JsonRequired]
        public double Importance { get; set; }

        [JsonRequired]
        public double Value { get; set; }
    }
}
