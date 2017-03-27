using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Entities
{
    public class RuleSetSettings
    {
        public string[] AssociatedWith { get; set; }

        public Dictionary<string, RuleLayerSettings> Layer { get; set; }
    }
}
