using System;
using System.Linq;
using System.Collections.Generic;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Helpers;

    //public delegate void RemovingEventHandler(object sender, RuleEventArgs e);

    public class RuleSet:IComparable<RuleSet>
    {
        int layerIndexer = 0;

        public int PositionNumber { get; set; }
        public List<RuleLayer> Layers { get; set; } 

        public Goal[] AssociatedWith { get; set; }

        public bool IsSequential { get; set; }

        private RuleSet(int number, Goal[] associatedGoals)
        {
            PositionNumber = number;
            Layers = new List<RuleLayer>();
            AssociatedWith = associatedGoals;
        }

        public RuleSet(int number, Goal[] associatedGoals, IEnumerable<RuleLayer> layers, bool isSequential) :this(number, associatedGoals)
        {
            layers.ForEach(l => Add(l));

            IsSequential = isSequential;
        }

        public void Add(RuleLayer layer)
        {
            layerIndexer++;
            layer.Set = this;
            layer.PositionNumber = layerIndexer;

            Layers.Add(layer);
        }


        public IEnumerable<Rule> AsRuleEnumerable()
        {
            return Layers.SelectMany(rl => rl.Rules);
        }

        public int CompareTo(RuleSet other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
