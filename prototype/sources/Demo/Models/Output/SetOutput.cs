using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class SetOutput
    {
        [JsonProperty("name")]
        public string SetName { get; set; }
        [JsonProperty("values")]
        public double[] Values { get; set; }
    }
}
