using System;
using System.Collections.Generic;

namespace SocialHuman.Models
{
    public sealed class AnticipatedInfluence
    {
        public Goal AssociatedGoal { get; private set; }

        public Heuristic AssociatedHeuristic { get; private set; }

        public double Value { get; set; }

        public AnticipatedInfluence(Heuristic associatedHeuristic, Goal associatedGoal, Dictionary<string,double> values)
        {
            AssociatedHeuristic = associatedHeuristic;
            AssociatedGoal = associatedGoal;

            string key = AssociatedGoal.Name;

            if (values != null)
            {
                Value = values.ContainsKey(key) ? values[key] : 0;
            }
            else
                Value = 0;

            
        }
    }
}
