using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class SiteOutput
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("activated")]
        public SetOutput[] ActivatedHeuristics { get; set; }
    }
}
