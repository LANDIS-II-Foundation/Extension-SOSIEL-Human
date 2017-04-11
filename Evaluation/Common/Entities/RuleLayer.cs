using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Helpers;   

    public class RuleLayer:IComparable<RuleLayer>
    {
        int RuleIndexer = 0;
        public int PositionNumber { get; set; }

        public RuleSet Set { get; set; }

        public RuleLayerSettings LayerSettings { get; private set; }

        public List<Rule> Rules { get; private set; }

        public RuleLayer(RuleLayerSettings settings)
        {
            Rules = new List<Rule>(settings.MaxNumberOfRules);
            LayerSettings = settings;
        }

        public RuleLayer(RuleLayerSettings parameters, IEnumerable<Rule> rules) : this(parameters)
        {
            rules.ForEach(r => Add(r));
        }

        public void Add(Rule Rule)
        {
            RuleIndexer++;
            Rule.RulePositionNumber = RuleIndexer;
            Rule.Layer = this;

            Rules.Add(Rule);
        }

        public int CompareTo(RuleLayer other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
