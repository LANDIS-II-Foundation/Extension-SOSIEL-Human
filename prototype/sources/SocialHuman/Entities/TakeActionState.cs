using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Entities
{
    public sealed class TakeActionState
    {
        public HeuristicSet Set { get; private set; }

        public double HarvestAmount { get; private set; }

        
        public TakeActionState(HeuristicSet set, double harvestAmount)
        {
            Set = set;
            HarvestAmount = harvestAmount;
        }
    }
}
