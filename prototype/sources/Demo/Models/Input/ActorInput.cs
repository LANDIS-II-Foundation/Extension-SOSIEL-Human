using Newtonsoft.Json;

namespace Demo.Models.Input
{
    sealed class ActorInput
    {
        [JsonProperty("actor_type")]
        public int ActorType { get; private set; }
        [JsonProperty("actor_classname")]
        public string ClassName { get; private set; }
        [JsonProperty("goal_value_tendency")]
        public string GoalTendency { get; private set; }
        [JsonProperty("assigned_sites")]
        public bool[] AssignedSites { get; private set; }
        [JsonProperty("heuristics_consequent_rules")]
        public HeuristicConsequentInput[] HeuristicsConsequentRules { get; private set; }
        [JsonProperty("heuristics")]
        public HeuristicInput[] HeuristicSet { get; private set; }
        [JsonProperty("matched_conditions_in_prior_period")]
        public string[][] MatchedConditionsInPriorPeriod { get; private set; }
        [JsonProperty("activated_heuristics_in_prior_period")]
        public string[][] ActivatedHeuristicsInPriorPeriod { get; private set; }

        
    }
}
