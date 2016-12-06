using System;

namespace SocialHuman.Models
{
    public sealed class PeriodInitialStateParameters
    {
        public double[] Harvested { get; private set; }
        public GoalStateParameters[] GoalsState { get; private set; }
        public string[][] MatchedConditionsInPriorPeriod { get; private set; }
        public string[][] ActivatedHeuristicsInPriorPeriod { get; private set; }
    }
}
