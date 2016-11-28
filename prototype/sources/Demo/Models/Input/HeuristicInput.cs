using Newtonsoft.Json;

namespace Demo.Models.Input
{
    sealed class HeuristicInput
    {
        [JsonProperty("heuristic_set")]
        public int Set { get; private set; }
        [JsonProperty("heuristic_layer")]
        public int Layer { get; private set; }
        [JsonProperty("heuristic_position_number")]
        public int PositionNumber { get; private set; }
        [JsonProperty("anticipated_influence")]
        public double AnticipatedInfluence { get; private set; }
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
}
