using System;
using System.Collections.Generic;

namespace SocialHuman.Models
{
    public sealed class AnticipatedInfluence : ICloneable
    {
        #region Public fields
        public Goal AssociatedGoal { get; private set; }

        public Heuristic AssociatedHeuristic { get; private set; }

        public double Value { get; set; }
        #endregion

        #region Constructors
        public AnticipatedInfluence(Heuristic associatedHeuristic, Goal associatedGoal, Dictionary<string, double> values)
        {
            AssociatedHeuristic = associatedHeuristic;
            AssociatedGoal = associatedGoal;

            string key = AssociatedGoal.Name;

            if (values != null)
            {
                Value = values.ContainsKey(key) ? values[key] : AssociatedHeuristic.GetConsequentValue();
            }
            else
                Value = AssociatedHeuristic.GetConsequentValue();
        }
        #endregion

        #region Public methods
        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
