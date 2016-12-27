using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Models
{
    using Enums;

    public sealed class GoalState
    {
        #region Private fields
        #endregion

        #region Public fields
        public Goal Goal { get; private set; }

        public double Value { get; set; }

        public double DiffCurrentAndMin { get; set; }

        public double DiffPriorAndMin { get; set; }

        public double AnticipatedInfluenceValue { get; set; }

        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }

        //public bool IsSelected { get; set; }
        #endregion

        #region Constructors
        public GoalState(Goal goal, double value)
        {
            Goal = goal;
            Value = value;
            //IsSelected = false;
            Confidence = true;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
