using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Entities
{
    public class Goal
    {
        public string Name { get; set; }

        public string Tendency { get; set; }

        public string ReferenceVariable { get; set; }

        public double FocalValue { get; set; }

        public bool ChangeFocalValueOnPrevious { get; set; }

        public string FocalValueReference { get; set; }

        public bool RankingEnabled { get; set; } = true;

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || ((Goal)obj).Name == Name;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
