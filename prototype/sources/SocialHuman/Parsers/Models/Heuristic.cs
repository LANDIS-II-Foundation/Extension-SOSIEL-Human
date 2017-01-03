using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    sealed class Heuristic
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("heuristic_set", Required = Required.Always)]
        public int Set { get; private set; }

        [JsonProperty("heuristic_layer", Required = Required.Always)]
        public int Layer { get; private set; }

        [JsonProperty("heuristic_position_number", Required = Required.Always)]
        public int PositionNumber { get; private set; }

        [JsonProperty("freshness_status", Required = Required.Always)]
        public int FreshnessStatus { get; private set; }

        [JsonProperty("is_action", Required = Required.Always)]
        public bool IsAction { get; private set; }

        [JsonProperty("antecedent", Required = Required.Always)]
        public HeuristicAntecedentPart[] Antecedent { get; private set; }

        [JsonProperty("consequent", Required = Required.Always)]
        public HeuristicConsequentPart Consequent { get; private set; }

        [JsonProperty("required_participants")]
        public int RequiredParticipants { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
