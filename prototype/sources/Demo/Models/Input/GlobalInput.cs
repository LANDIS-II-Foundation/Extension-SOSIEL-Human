using Newtonsoft.Json;

namespace Demo.Models.Input
{
    sealed class GlobalInput
    {
        [JsonProperty("biomass_by_site")]
        public double[] BiomassBySite { get; private set; }
        [JsonProperty("periods_count")]
        public int PeriodsCount { get; private set; }
        [JsonProperty("biomass_growth_rate")]
        public double[] BiomassGrowthRate { get; private set; }
        [JsonProperty("historical_total_biomass_min")]
        public double HistoricalTotalBiomassMin { get; private set; }
        [JsonProperty("max_heuristic_in_layer")]
        public int MaxHeuristicInLayer { get; private set; }
        [JsonProperty("power_of_distribution")]
        public int PowerOfDistribution { get; private set; }
    }
}
