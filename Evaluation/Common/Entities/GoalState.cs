using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public sealed class GoalState
    {
        public Goal Goal { get; private set; }

        public double Value { get; set; }

        public double DiffCurrentAndMin { get; set; }

        public double DiffPriorAndMin { get; set; }

        public double AnticipatedInfluenceValue { get; set; }

        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }

        //public bool IsSelected { get; set; }

        public GoalState(Goal goal, double value)
        {
            Goal = goal;
            Value = value;
            //IsSelected = false;
            Confidence = true;
        }

    }
}
