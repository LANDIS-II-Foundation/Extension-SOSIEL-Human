using System;
using Newtonsoft.Json;

namespace Demo.Models.Input
{
    using SocialHuman.Enums;

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
}
