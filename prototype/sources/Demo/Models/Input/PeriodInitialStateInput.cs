using System;
using Newtonsoft.Json;

namespace Demo.Models.Input
{
    sealed class PeriodInitialStateInput
    {
        [JsonProperty("harvested")]
        public double[] Harvested { get; private set; }
        [JsonProperty("goals_state")]
        public GoalStateInput[] GoalsState { get; private set; }
        [JsonProperty("matched_conditions_in_prior_period")]
        public string[][] MatchedConditionsInPriorPeriod { get; private set; }
        [JsonProperty("activated_heuristics_in_prior_period")]
        public string[][] ActivatedHeuristicsInPriorPeriod { get; private set; }
    }

    sealed class GoalStateInput
    {
        [JsonProperty("name")]
        public string GoalName { get; private set; }
        [JsonProperty("value")]
        public double GoalValue { get; private set; }
    }
}
