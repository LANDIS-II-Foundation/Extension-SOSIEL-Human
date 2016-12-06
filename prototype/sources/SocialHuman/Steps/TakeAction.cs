using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Actors;
    using Entities;
    using Models;

    sealed class TakeAction
    {
        public void Execute(Actor actor, LinkedListNode<Period> periodModel)
        {
            Period currentPeriod = periodModel.Value;

            foreach (SiteState siteState in currentPeriod.SiteStates[actor])
            {
                Heuristic[] activatedHeuristics = siteState.Activated;

                foreach (var set in activatedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                {
                    double consequentValue = set.OrderBy(h => h.Layer.PositionNumber).Last().ConsequentValue;

                    TakeActionState takeActionState = new TakeActionState(set.Key, siteState.Site.BiomassValue * (consequentValue / 100));

                    siteState.Site.BiomassValue -= takeActionState.HarvestAmount;

                    if (siteState.Site.BiomassValue <= 0)
                    {
                        currentPeriod.IsOverconsumption = true;
                    }

                    siteState.TakeActions.Add(takeActionState);
                }

            }
        }
    }
}
