using System;
using Newtonsoft.Json;

namespace Demo.Models.Input
{
    using SocialHuman.Enums;

    sealed class ActorInput
    {
        [JsonProperty("actor_type")]
        public int ActorType { get; private set; }
        [JsonProperty("actor_name")]
        public string ActorName { get; private set; }
        [JsonProperty("actor_classname")]
        public string ClassName { get; private set; }
        [JsonProperty("assigned_sites")]
        public bool[] AssignedSites { get; private set; }
        [JsonProperty("goals")]
        public ActorGoalInput[] Goals { get; private set; }
        [JsonProperty("heuristics_consequent_rules")]
        public HeuristicConsequentInput[] HeuristicConsequentRules { get; private set; }
        [JsonProperty("heuristics")]
        public HeuristicInput[] Heuristics { get; private set; }
    }

    sealed class HeuristicInput
    {
        [JsonProperty("heuristic_set")]
        public int Set { get; private set; }
        [JsonProperty("heuristic_layer")]
        public int Layer { get; private set; }
        [JsonProperty("heuristic_position_number")]
        public int PositionNumber { get; private set; }
        [JsonProperty("anticipated_influence")]
        public double[] AnticipatedInfluence { get; private set; }
        [JsonProperty("freshness_status")]
        public int FreshnessStatus { get; private set; }
        [JsonProperty("is_action")]
        public bool IsAction { get; private set; }

        [JsonProperty("antecedent_const")]
        public double AntecedentConst { get; private set; }
        [JsonProperty("antecedent_inequality_sign")]
        public string AntecedentInequalitySign { get; private set; }
        [JsonProperty("consequent_value")]
        public double ConsequentValue { get; private set; }
    }

    sealed class HeuristicConsequentInput
    {
        [JsonProperty("heuristic_layer")]
        public int HeuristicLayer { get; private set; }
        [JsonProperty("consequent_value_interval")]
        public int[] ConsequentValueInterval { get; set; }
        [JsonProperty("consequent_relationship_with_goal_variable")]
        public string ConsequentRelationshipStr { get; set; }

        public ConsequentRelationship ConsequentRelationshipConverter
        {
            get
            {
                switch (ConsequentRelationshipStr)
                {
                    case "+":
                        return ConsequentRelationship.Positive;
                    case "-":
                        return ConsequentRelationship.Negative;

                    default:
                        //todo
                        throw new Exception("trouble with relationship");
                }
            }

        }
    }

    sealed class ActorGoalInput
    {
        [JsonProperty("is_primary")]
        public bool IsPrimary { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("comment")]
        public string Comment { get; private set; }
        [JsonProperty("tendency")]
        public string Tendency { get; private set; }
        [JsonProperty("min")]
        public double MinValue { get; private set; }
    }
}
