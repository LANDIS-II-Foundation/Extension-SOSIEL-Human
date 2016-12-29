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
        public string LayerName { get; set; }

        [JsonProperty("heuristics")]
        public HeuristicOutput[] Heuristics { get; set; }
        #endregion
    }
}
