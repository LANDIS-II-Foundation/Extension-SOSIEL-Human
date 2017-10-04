namespace ArtificialVCM.Output
{
    public class AgentDetailsOutput
    {
        public const string FileName = "{0}_details.csv";

        public int Iteration { get; set; }

        public string AgentId { get; set; }

        public string AgentProfile { get; set; }

        public double G1Importance { get; set; }

        public double G2Importance { get; set; }

        public double AgentContribution { get; set; }

        public int NumberOfKH { get; set; }

        public string SelectedGoal { get; set; }

        public string Details { get; set; }
    }
}