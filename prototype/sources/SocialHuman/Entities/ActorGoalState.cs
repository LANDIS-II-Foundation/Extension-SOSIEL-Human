using System;
using System.Linq;

namespace SocialHuman.Entities
{
    using Enums;

    public sealed class ActorGoalState
    {
        public ActorGoal Goal { get; private set; }

        public double Value { get; set; }

        public double DiffCurrentAndMin { get; set; }

        public double DiffPriorAndMin { get; set; }

        public double AnticipatedInfluenceValue { get; set; }
        public bool Confidence { get; set; }
        public AnticipatedDirection AnticipatedDirection { get; set; }

        public bool IsSelected { get; set; }

        public ActorGoalState(ActorGoal goal, double value)
        {
            Goal = goal;
            Value = value;
            IsSelected = false;
            Confidence = true;
        }
    }
}
