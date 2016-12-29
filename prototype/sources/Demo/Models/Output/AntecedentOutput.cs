using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Demo.Models.Output
{
    class AntecedentOutput
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("ancetedent")]
        public double AntecedentConst { get; set; }

        [JsonProperty("sign")]
        public string AntecedentSign { get; set; }

        [JsonProperty("param")]
        public string AntecedentParam { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
