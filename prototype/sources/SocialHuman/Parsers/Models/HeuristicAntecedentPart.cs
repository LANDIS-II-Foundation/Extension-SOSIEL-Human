using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    sealed class HeuristicAntecedentPart
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("immutable")]
        public bool Immutable { get; private set; } = true;
        [JsonProperty("param", Required = Required.Always)]
        public string Param { get; private set; }
        [JsonProperty("sign", Required = Required.Always)]
        public string Sign { get; private set; }
        [JsonProperty("const")]
        public dynamic Const { get; private set; }
        [JsonProperty("link_for_const")]
        public string LinkForConst { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
