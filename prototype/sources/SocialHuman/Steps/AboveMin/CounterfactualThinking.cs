using System;
using System.Linq;

namespace SocialHuman.Steps.AboveMin
{
    using Entities;
    using Enums;
    using Abstract;

    sealed class CounterfactualThinking : CounterfactualThinkingAbstract
    {
        #region Constructors
        public CounterfactualThinking()
        {
            Tendency = TendencyNames.AboveMin;
        }


        #endregion


        #region Override methods
        protected override bool SpecificLogic(ActorGoalState criticalGoalState, HeuristicLayer layer,
            Heuristic[] matchedPriorPeriodHeuristics, Heuristic priorPeriodHeuristic)
        {
            ActorGoal criticalGoal = criticalGoalState.Goal;

            Heuristic[] resultHeuristics = layer.Heuristics
                .Where(h => h.ForGoal(criticalGoal).Value >= 0 && h.ForGoal(criticalGoal).Value > criticalGoalState.DiffCurrentAndMin)
                .ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (resultHeuristics.Length == 0)
            {
                return false;
            }
            else
            {
                resultHeuristics = resultHeuristics.GroupBy(h => h.ForGoal(criticalGoal).Value).OrderBy(h => h.Key).First().ToArray();

                return resultHeuristics.Any(h => !(h == priorPeriodHeuristic || h.IsAction == false));
            }
        }
        #endregion

    }
}
