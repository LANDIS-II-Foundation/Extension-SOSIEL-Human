using Newtonsoft.Json;

namespace SocialHuman.Parsers.Models
{
    sealed class GlobalInput
    {
        #region Public fields
        [JsonProperty("biomass_by_site", Required = Required.Always)]
        public double[] BiomassBySite { get; private set; }
        [JsonProperty("periods_count", Required = Required.Always)]
        public int PeriodsCount { get; private set; }
        [JsonProperty("biomass_growth_rate", Required = Required.Always)]
        public double[] BiomassGrowthRate { get; private set; }
        [JsonProperty("power_of_distribution", Required = Required.Always)]
        public int PowerOfDistribution { get; private set; }
        #endregion
    }
}
