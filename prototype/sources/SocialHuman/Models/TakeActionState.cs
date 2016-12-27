using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    public sealed class TakeActionState
    {
        public string VariableName { get; private set; }

        public double Value { get; private set; }

        //public HeuristicSet Set { get; private set; }

        //public double HarvestAmount { get; private set; }

        public TakeActionState(string variableName, double value)
        {
            VariableName = variableName;
            Value = value;
        }
    }
}
