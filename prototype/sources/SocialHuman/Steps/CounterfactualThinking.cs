using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Models;

    class CounterfactualThinking : VolatileStep
    {
        #region Public fields
        bool? confidence;

        GoalState criticalGoalState;
        //HeuristicLayer layer;
        AnticipatedInfluence[] anticipatedInfluences;

        Heuristic activatedHeuristic;
        #endregion


        #region Private methods

        #endregion

        #region Override methods
        protected override void AboveMin()
        {
            AnticipatedInfluence[] result = anticipatedInfluences
                .Where(ai => ai.Value >= 0 && ai.Value > criticalGoalState.DiffCurrentAndMin).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (result.Length == 0)
            {
                confidence = false;
            }
            else
            {
                result = result.GroupBy(ai => ai.Value).OrderBy(h => h.Key).First().ToArray();

                confidence = result.Any(ai => !(ai.AssociatedHeuristic == activatedHeuristic || ai.AssociatedHeuristic.IsAction == false));
            }
        }

        protected override void BelowMax()
        {
            AnticipatedInfluence[] result = anticipatedInfluences
                .Where(ai => ai.Value < 0 && Math.Abs(ai.Value) > Math.Abs(criticalGoalState.DiffCurrentAndMin)).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (result.Length == 0)
            {
                confidence = false;
            }
            else
            {
                result = result.GroupBy(ai => ai.Value).OrderBy(h => h.Key).First().ToArray();

                confidence = result.Any(ai => !(ai.AssociatedHeuristic == activatedHeuristic || ai.AssociatedHeuristic.IsAction == false));
            }
        }
        #endregion

        #region Public methods
        public bool? Execute(Actor actor, LinkedListNode<Period> periodModel, GoalState criticalState, 
            Heuristic[] matched, Site site, HeuristicLayer layer)
        {
            confidence = null;

            //Period currentPeriod = periodModel.Value;
            Period priorPeriod = periodModel.Previous.Value;

            criticalGoalState = criticalState;

            

            if (criticalGoalState.Confidence == false && matched.Length >= 2)
            {
                //criticalGoalState = currentPeriod.GetCriticalGoalState(actor);
                activatedHeuristic = priorPeriod.GetStateForSite(actor, site).GetActivated(layer);

                anticipatedInfluences = actor.AnticipatedInfluences
                                .Where(ai => matched.Contains(ai.AssociatedHeuristic) && ai.AssociatedGoal == criticalGoalState.Goal)
                                .ToArray();

                SpecificLogic(criticalGoalState.Goal.Tendency);
            }

            return confidence;
        }


        #endregion
    }
}
