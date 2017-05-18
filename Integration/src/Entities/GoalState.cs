using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Enums;

    public sealed class GoalState
    {
        public double Value { get; set; }

        public double FocalValue { get; set; }

        public double DiffCurrentAndFocal { get; set; }

        public double DiffPriorAndFocal { get; set; }

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
