using System;
using System.Linq;

namespace SocialHuman.Steps.AboveMin
{
    using Abstract;
    using Entities;
    using Models;
    using Randoms;
    

    sealed class HeuristicSelection : HeuristicSelectionAbstract
    {
        #region Override methods
        protected override Heuristic ChooseOne(ActorGoalState criticalGoal, HeuristicLayer layer, Heuristic[] matchedHeuristics)
        {
            if (criticalGoal.DiffCurrentAndMin > 0)
                return priorPeriodSiteState.GetActivated(layer);
            else
            {
                ActorGoal goal = criticalGoal.Goal;

                Heuristic[] selectedHeuristics = matchedHeuristics.Where(h => h.ForGoal(goal).Value >= 0 &&
                    h.ForGoal(goal).Value > criticalGoal.DiffCurrentAndMin).ToArray();

                //if none are identified, then choose the do-nothing heuristic.
                if (selectedHeuristics.Length == 0)
                    return matchedHeuristics.Single(h => h.IsAction == false);
                else
                    selectedHeuristics = selectedHeuristics.GroupBy(h => h.ForGoal(goal).Value).OrderBy(hg => hg.Key).First().ToArray();

                if (selectedHeuristics.Length == 1)
                    return selectedHeuristics[0];
                else
                {
                    return selectedHeuristics[LinearUniformRandom.GetInstance.Next(selectedHeuristics.Length)];
                }
            }
        }
        #endregion
    }
}
