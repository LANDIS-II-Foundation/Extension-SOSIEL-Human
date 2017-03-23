using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Entities
{
    using Helpers;

    //public delegate void RemovingEventHandler(object sender, RuleEventArgs e);

    public class RuleSet
    {
        int layerIndexer = 0;

        public int PositionNumber { get; set; }
        public List<RuleLayer> Layers { get; set; } 

        //public Goal[] AssociatedWith { get; private set; }

        private RuleSet(int number/*, Goal[] goals*/)
        {
            PositionNumber = number;
            Layers = new List<RuleLayer>();
            //AssociatedWith = goals;
        }

        public RuleSet(int number, IEnumerable<RuleLayer> layers):this(number)
        {
            layers.ForEach(l => Add(l));
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
             

        

        //public void UnassignRule(Rule Rule)
        //{
        //    OnRemovingRule(this, new RuleEventArgs(Rule));
        //}

        //public event RemovingEventHandler OnRemovingRule;
    }
}
