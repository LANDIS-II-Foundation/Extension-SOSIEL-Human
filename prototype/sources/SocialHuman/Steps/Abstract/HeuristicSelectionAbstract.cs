using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps.Abstract
{
    using Actors;
    using Entities;
    using Models;

    abstract class HeuristicSelectionAbstract
    {
        #region Private fields
        protected SiteState priorPeriodSiteState;
        #endregion

        #region Abstract methods
        protected abstract Heuristic ChooseOne(ActorGoalState criticalGoal, HeuristicLayer layer, Heuristic[] matchedHeuristics);
        #endregion

        #region Public methods
        public void Execute(Actor actor, LinkedListNode<Period> periodModel, Site site, HeuristicSet set)
        {
            Period currentPeriod = periodModel.Value, 
                priorPeriod = periodModel.Previous.Value;

            List<Heuristic> matched = new List<Heuristic>();
            List<Heuristic> activated = new List<Heuristic>();

            double result = currentPeriod.TotalBiomass;

            foreach (HeuristicLayer layer in set.Layers)
            {
                priorPeriodSiteState = priorPeriod.GetStateForSite(actor, site);
                Heuristic[] matchedForLayer = layer.Heuristics.Where(h => h.IsMatch(result)).ToArray();

                Heuristic activatedHeuristic;

                if (matchedForLayer.Length > 1)
                    activatedHeuristic = ChooseOne(currentPeriod.GetCriticalGoal(actor), layer, matchedForLayer);
                else
                    activatedHeuristic = matchedForLayer[0];

                activatedHeuristic.FreshnessStatus = 0;

                result = activatedHeuristic.ConsequentValue;

                activated.Add(activatedHeuristic);
                matched.AddRange(matchedForLayer);
            }

            SiteState newSiteState = SiteState.Create(currentPeriod.GetCurrentSiteState(site), matched.ToArray(), activated.ToArray());

            currentPeriod.SiteStates[actor].Add(newSiteState);
        }
        #endregion
    }
}
