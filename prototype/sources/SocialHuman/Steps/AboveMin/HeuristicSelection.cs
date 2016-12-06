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
                    h.ForGoal(goal).Value > criticalGoal.DiffCurrentAndMin)
                    .GroupBy(h => h.ForGoal(goal).Value).OrderBy(hg => hg.Key).First().ToArray();

                if (selectedHeuristics.Length == 1)
                    return selectedHeuristics[0];
                else
                {
                    if (selectedHeuristics.Length == 0)
                        throw new Exception("Heuristic didn't found on HS step");

                    return selectedHeuristics[LinearUniformRandom.GetInstance.Next(selectedHeuristics.Length)];
                }
            }
        }
        #endregion
    }
}
