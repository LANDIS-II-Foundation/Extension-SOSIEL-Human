namespace SocialHuman.Models
{
    using Enums;
    public sealed class HeuristicConsequentRule
    {
        public int HeuristicLayer { get; private set; }
        public int[] ConsequentValueInterval { get; private set; }
        public ConsequentRelationship ConsequentRelationship { get; private set; }

        public int MinValue
        {
            get
            {
                return ConsequentValueInterval[0];
            }
        }

        public int MaxValue
        {
            get
            {
                return ConsequentValueInterval[1];
            }
        }
    }
}
