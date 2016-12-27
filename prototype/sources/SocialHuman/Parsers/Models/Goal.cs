using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    sealed class Goal
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("is_primary", Required = Required.Always)]
        public bool IsPrimary { get; private set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty("comment", Required = Required.Always)]
        public string Comment { get; private set; }

        [JsonProperty("tendency", Required = Required.Always)]
        public string Tendency { get; private set; }

        [JsonProperty("limit_value", Required = Required.Always)]
        public double LimitValue { get; private set; }

        [JsonProperty("increased", Required = Required.Always)]
        public bool Increased { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
