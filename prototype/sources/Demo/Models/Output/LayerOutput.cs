using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace Demo.Models.Output
{
    class LayerOutput
    {
        #region Public fields
        [JsonProperty("name")]
        public string HeuristicName { get; set; }
        [JsonProperty("ancetedent")]
        public double AntecedentConst { get; set; }
        [JsonProperty("sign")]
        public string AntecedentSign { get; set; }
        [JsonProperty("consequent")]
        public double ConsequentValue { get; set; }
        #endregion
    }
}
