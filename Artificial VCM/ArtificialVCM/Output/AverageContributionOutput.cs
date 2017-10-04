namespace ArtificialVCM.Output
{
    public class AverageContributionOutput
    {
        public const string FileName = "Average_contribution.csv";

        public int Iteration { get; set; }

        public double AverageContribution { get; set; }
    }
}