using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SocialHuman.Parsers.Models
{
    sealed class ActorPrototype
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("actor_type", Required = Required.Always)]
        public int Type { get; private set; }

        [JsonProperty("variables")]
        public Dictionary<string, dynamic> Variables { get; private set; }

        [JsonProperty("goals", Required = Required.Always)]
        public Goal[] Goals { get; private set; }

        [JsonProperty("heuristics", Required = Required.Always)]
        public Heuristic[] Heuristics { get; private set; }

        [JsonProperty("heuristic_layer_parameters", Required = Required.Always)]
        public HeuristicLayerParameters[] HeuristicLayerParameters { get; private set; }

        [JsonProperty("heuristic_set_parameters")]
        public HeuristicSetParameters[] HeuristicSetParameters { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
