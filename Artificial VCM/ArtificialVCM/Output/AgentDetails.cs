namespace ArtificialVCM.Output
{
    public class AgentDetails
    {
        public const string FileName = "Agent_details.csv";

        public int Iteration { get; set; }

        public string AgentProfile { get; set; }

        public double G1Importance { get; set; }

        public double G2Importance { get; set; }

        public double AgentContribution { get; set; }

        public double ConsequentValue { get; set; }

        public int NumberOfKH { get; set; }
    }
}