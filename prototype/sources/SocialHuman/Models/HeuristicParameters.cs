namespace SocialHuman.Models
{
    public sealed class HeuristicParameters: AntecedentParameters
    {
        public double ConsequentValue { get; set; }

        public int Set { get; set; }
        public int Layer { get; set; }
        public int PositionNumber { get; set; }
        public double AnticipatedInfluence { get; set; }
        public int FreshnessStatus { get; set; }
        public bool IsAction { get; set; }
    }
}
