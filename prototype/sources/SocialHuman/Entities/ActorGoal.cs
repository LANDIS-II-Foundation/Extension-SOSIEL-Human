using System;

namespace SocialHuman.Entities
{
    public sealed class ActorGoal
    {
        public bool IsPrimary { get; private set; }
        public string Name { get; private set; }
        public string Comment { get; private set; }
        public string Tendency { get; private set; }
        public double MinValue { get; set; }
    }
}
