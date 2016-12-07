using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class SetOutput 
    {
        [JsonProperty("name")]
        public string SetName { get; set; }
        [JsonProperty("heuristics")]
        public LayerOutput[] ActivatedHeuristics { get; set; }
        [JsonProperty("harvested")]
        public double Harvested { get; set; }
    }
}
