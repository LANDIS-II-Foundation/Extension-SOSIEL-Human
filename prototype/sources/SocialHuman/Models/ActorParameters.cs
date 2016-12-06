namespace SocialHuman.Models
{
    using Entities;
    using Enums;

    public sealed class ActorParameters
    {
        public ActorType ActorType { get; private set; }
        public string ActorName { get; private set; }
        public string ClassName { get; private set; }
        public bool[] AssignedSites { get; private set; }
        public ActorGoal[] Goals { get; private set; }
        public HeuristicConsequentRule[] HeuristicConsequentRules { get; private set; }
        public HeuristicParameters[] Heuristics { get; private set; }

        //public PeriodInitialStateParameters InitialState { get; private set; }
    }
}
