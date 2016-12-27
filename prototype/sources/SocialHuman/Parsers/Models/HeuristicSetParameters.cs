using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SocialHuman.Parsers.Models
{
    sealed class HeuristicSetParameters
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("heuristic_set")]
        public int HeuristicSet { get; private set; } 

        [JsonProperty("associated_with_goal")]
        public string[] AssociatedWith { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
