using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Helpers;   

    public class RuleLayer
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

        void CheckAndRemove()
        {
            if (Rules.Count == LayerSettings.MaxNumberOfRules)
            {
                Rule oldestRule = Rules.OrderByDescending(h => h.FreshnessStatus).First(h => h.IsAction == true);
                oldestRule.Layer = null;
                //Set.UnassignRule(oldestRules);

                Rules.Remove(oldestRule);
            }
        }

        public void Add(Rule Rule)
        {
            CheckAndRemove();

            RuleIndexer++;
            Rule.RulePositionNumber = RuleIndexer;
            Rule.Layer = this;

            Rules.Add(Rule);
        }
    }
}
