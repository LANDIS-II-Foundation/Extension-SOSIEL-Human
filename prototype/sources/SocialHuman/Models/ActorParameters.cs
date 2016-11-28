namespace SocialHuman.Models
{
    using Entities;
    using Enums;

    public sealed class ActorParameters
    {
        public ActorType ActorType { get; private set; }
        public string ClassName { get; private set; }
        public string GoalTendency { get; private set; }
        public HeuristicConsequentRule[] HeuristicsConsequentRules { get; private set; }
        public HeuristicParameters[] Heuristics { get; private set; }
        public string[][] MatchedConditionsInPriorPeriod { get; private set; }
        public string[][] ActivatedHeuristicsInPriorPeriod { get; private set; }
        public bool[] AssignedSites { get; private set; }
    }
}
