using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace Demo.Models.Output
{
    class Output
    {
        #region Public fields
        [JsonProperty("periods")]
        public PeriodOutput[] Periods { get; set; }
        [JsonProperty("mental_models")]
        public dynamic[] MentalModels { get; set; }
        #endregion
    }
}
