using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    sealed class GoalState
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }
        [JsonProperty("value", Required = Required.Always)]
        public double Value { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
