using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Entities
{
    public class Goal
    {
        public string Name { get; set; }

        public string Tendency { get; set; }

        public string ReferenceVariable { get; set; }

        public double FocalValue { get; set; }

        public double MaxGoalValue { get; set; }

        public bool ChangeFocalValueOnPrevious { get; set; } 
    }
}
