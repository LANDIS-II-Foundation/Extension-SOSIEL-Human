using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class SetOutput 
    {
        [JsonProperty("name")]
        public string SetName { get; set; }

        [JsonProperty("layers_act")]
        public LayerOutput[] ActivatedHeuristics { get; set; }

        [JsonProperty("layers")]
        public LayerOutput[] Layers { get; set; } 

        [JsonProperty("take_actions")]
        public TakeActionOutput[] TakeActions { get; set; }
    }
}
