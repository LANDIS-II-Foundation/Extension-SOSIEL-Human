namespace SHELite.Output
{
    public class AgentDetailsOutput
    {
        public const string FileName = "{0}_details.csv";

        public int Iteration { get; set; }

        public string AgentId { get; set; }

        public int Age { get; set; }

        public int NumberOfKH { get; set; }

        public double Income { get; set; }

        public double Expenses { get; set; }

        public double Savings { get; set; }
    }
}