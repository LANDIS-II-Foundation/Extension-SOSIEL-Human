using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class TakeActionOutput
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("param")]
        public string Param { get; set; }

        [JsonProperty("value")]
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
