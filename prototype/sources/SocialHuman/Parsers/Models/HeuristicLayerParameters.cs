using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    using Enums;

    sealed class HeuristicLayerParameters
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("heuristic_set", Required = Required.Always)]
        public int HeuristicSet { get; private set; }

        [JsonProperty("heuristic_layer", Required = Required.Always)]
        public int HeuristicLayer { get; private set; }

        [JsonProperty("modifiable")]
        public bool Modifiable { get; private set; } = true;

        [JsonProperty("max_heuristic_count", Required = Required.Always)]
        public int MaxHeuristicCount { get; private set; }

        [JsonProperty("consequent_value_interval")]
        public int[] ConsequentValueInterval { get; private set; }

        [JsonProperty("consequent_relationship_with_goal_variable")]
        public string ConsequentRelationshipStr { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
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
        #endregion
    }
}
