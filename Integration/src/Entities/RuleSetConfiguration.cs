using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Entities
{
    public class RuleSetConfiguration
    {
        public string[] AssociatedWith { get; set; }

        public bool IsSequential { get; set; }

        public Dictionary<string, RuleLayerConfiguration> Layer { get; set; }
    }
}
