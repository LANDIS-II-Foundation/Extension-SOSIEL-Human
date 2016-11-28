using Newtonsoft.Json;

namespace Demo.Models.Output
{
    class PeriodOutput
    {
        [JsonProperty("period_number")]
        public int PeriodNumber { get; set; }
        [JsonProperty("biomass")]
        public double[] Biomass { get; set; }
        [JsonProperty("actors")]
        public ActorOutput[] Actors { get; set; }
    }
}
