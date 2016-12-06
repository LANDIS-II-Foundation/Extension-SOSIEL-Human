using System;
using System.Linq;

namespace SocialHuman.Steps.AboveMin
{
    using Entities;
    using Enums;
    using Abstract;
    using Models;


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

            Heuristic resultHeuristic = layer.Heuristics
                .Where(h => h.ForGoal(criticalGoal).Value >= 0 && h.ForGoal(criticalGoal).Value > criticalGoalState.DiffCurrentAndMin)
                .OrderBy(h => h.ForGoal(criticalGoal).Value).First();

            bool isPriorPeriod = resultHeuristic == priorPeriodHeuristic;

            return !(isPriorPeriod || resultHeuristic.IsAction == false);
        }
        #endregion

    }
}
