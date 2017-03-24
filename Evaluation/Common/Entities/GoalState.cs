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

        public double AnticipatedInfluenceValue { get; set; }

        public double Coef { get; set; }

        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }

        //public bool IsSelected { get; set; }

        public GoalState(double value, double focalValue, double coef)
        {
            Value = value;
            FocalValue = focalValue;
            Coef = coef;
            //IsSelected = false;
            Confidence = true;
        }

        public GoalState CreateForNextIteration()
        {
            return new GoalState(Value, FocalValue, Coef);
        }

    }
}
