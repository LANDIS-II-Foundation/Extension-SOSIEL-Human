using System;

namespace SocialHuman.Entities
{
    public class AnticipatedInfluence
    {
        public ActorGoal ActorGoal { get; private set; }
        public double Value { get; set; }

        public AnticipatedInfluence(ActorGoal actorGoal, double value)
        {
            ActorGoal = actorGoal;
            Value = value;
        }
    }
}
