using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class ActorOutput
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("information")]
        public SiteOutput[] Information { get; set; }
    }
}
