using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    sealed class HeuristicConsequentPart
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("param", Required = Required.Always)]
        public string Param { get; private set; }
        [JsonProperty("value")]
        public double Value { get; private set; }
        [JsonProperty("link_to_value")]
        public string LinkToValue { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
