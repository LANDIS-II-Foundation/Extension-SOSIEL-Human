using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    sealed class InitialState: Actor
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("matched_conditions_in_prior_period", Required = Required.Always)]
        public string[][] MatchedConditionsInPriorPeriod { get; private set; }
        [JsonProperty("activated_heuristics_in_prior_period", Required = Required.Always)]
        public string[][] ActivatedHeuristicsInPriorPeriod { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
