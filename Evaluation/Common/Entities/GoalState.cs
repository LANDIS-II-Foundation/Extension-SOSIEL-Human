using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public sealed class GoalState
    {
        public double Value { get; set; }

        public double FocalValue { get; set; }

        public double DiffCurrentAndMin { get; set; }

        public double DiffPriorAndMin { get; set; }

        public double DiffPriorAndCurrent { get; set; }

        public double AnticipatedInfluenceValue { get; set; }

        public double Proportion { get; set; }

        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }


        public GoalState(double value, double focalValue, double proportion)
        {
            Value = value;
            FocalValue = focalValue;
            Proportion = proportion;
            Confidence = true;
        }

        public GoalState CreateForNextIteration()
        {
            return new GoalState(Value, FocalValue, Proportion);
        }

    }
}
