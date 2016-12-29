using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Demo.Models.Output
{
    class HeuristicOutput
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("name")]
        public string HeuristicName { get; set; }
        
        [JsonProperty("antecedents")]
        public AntecedentOutput[] Antecedents { get; set; }

        [JsonProperty("consequent_param")]
        public string ConsequentParam { get; set; }

        [JsonProperty("consequent")]
        public double ConsequentValue { get; set; }

        [JsonProperty("is_action")]
        public bool IsAction { get; set; }

        [JsonProperty("is_collective")]
        public bool isCollective { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
