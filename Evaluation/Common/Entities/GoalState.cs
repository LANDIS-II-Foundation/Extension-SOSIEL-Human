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

        public double DiffCurrentAndMax { get; set; }

        public double DiffPriorAndMin { get; set; }

        public double DiffPriorAndMax { get; set; }

        public double DiffPriorAndCurrent { get; set; }

        public double AnticipatedInfluenceValue { get; set; }


        public double Importance { get; set; }

        public double AdjustedImportance { get; set; }
        
        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }


        public GoalState(double value, double focalValue, double importance)
        {
            Value = value;
            FocalValue = focalValue;
            Importance = importance;
            Confidence = true;
        }

        public GoalState CreateForNextIteration()
        {
            return new GoalState(Value, FocalValue, Importance);
        }

    }
}
