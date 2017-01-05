using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class HouseholdOutput
    {
        #region Private fields
        #endregion

        #region Public fields
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("income")]
        public double Income { get; set; }

        [JsonProperty("expenses")]
        public double Expenses { get; set; }

        [JsonProperty("savings")]
        public double Savings { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
