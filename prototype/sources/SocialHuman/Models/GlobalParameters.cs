namespace SocialHuman.Models
{
    public class GlobalParameters
    {
        public double[] BiomassBySite { get; internal set; }
        public int PeriodsCount { get; private set; }
        public double[] BiomassGrowthRate { get; private set; }
        public double HistoricalTotalBiomassMin { get; private set; }
        public int MaxHeuristicInLayer { get; private set; }
        public int PowerOfDistribution { get; private set; }
    }
}
