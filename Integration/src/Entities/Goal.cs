using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Entities
{
    public class Goal
    {
        public string Name { get; private set; }

        public string Tendency { get; private set; }

        public string ReferenceVariable { get; private set; }

        public double FocalValue { get; private set; }

        public bool ChangeFocalValueOnPrevious { get; private set; }

        public double ReductionPercent { get; private set; }

        public string FocalValueReference { get; private set; }

        public bool RankingEnabled { get; private set; } 


        public Goal()
        {
            RankingEnabled = true;
        }

        public override bool Equals(object obj)
        {
            //check on equality by object reference or goal name
            return base.Equals(obj) || ((Goal)obj).Name == Name;
        }

        public override int GetHashCode()
        {
            //turn off checking by hash code
            return 0;
        }
    }
}
