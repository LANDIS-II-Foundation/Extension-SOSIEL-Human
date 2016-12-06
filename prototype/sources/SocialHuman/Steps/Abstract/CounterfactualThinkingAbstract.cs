using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps.Abstract
{
    using Abstract;
    using Actors;
    using Entities;
    using Models;

    abstract class CounterfactualThinkingAbstract
    {
        #region Public fields
        public string Tendency { get; protected set; }
        #endregion


        #region Abstract methods
        protected abstract bool SpecificLogic(ActorGoalState criticalGoalState, HeuristicLayer layer, Heuristic[] matchedPriorPeriodHeuristics, Heuristic priorPeriodHeuristic);
        #endregion

        #region Public methods
        public bool Execute(Actor actor, LinkedListNode<Period> periodModel, Site site, HeuristicLayer layer)
        {
            Period currentPeriod = periodModel.Value;

            Period priorPeriod = periodModel.Previous.Value;

            Heuristic[] matchedPriorPeriodHeuristics = priorPeriod.GetStateForSite(actor, site).
                Matched.Where(h=>h.Layer == layer).ToArray();

            ActorGoalState criticalGoalState = currentPeriod.GetCriticalGoal(actor);

            if (criticalGoalState.Confidence == false && matchedPriorPeriodHeuristics.Length >= 2)
            {
                Heuristic activatedPriorPeriodHeuristic = priorPeriod.GetStateForSite(actor, site).GetActivated(layer);

                return SpecificLogic(criticalGoalState, layer, matchedPriorPeriodHeuristics, activatedPriorPeriodHeuristic);
            }

            return true;
        }
        #endregion
    }
}
