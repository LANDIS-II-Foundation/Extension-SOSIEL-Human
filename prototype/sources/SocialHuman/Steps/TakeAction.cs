using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Enums;
    using Models;

    sealed class TakeAction
    {
        public void Execute(Actor actor, LinkedListNode<Period> periodModel, Site[] sites)
        {
            Period currentPeriod = periodModel.Value;

            foreach (Site site in currentPeriod.GetAssignedSites(actor))
            {
                foreach (var set in currentPeriod.GetStateForSite(actor, site).Activated.GroupBy(h => h.Layer.Set))
                {
                    //take last layer activated heuristic
                    Heuristic heuristic = set.OrderByDescending(h => h.Layer.PositionNumber).First();
                    //Heuristic heuristic = currentPeriod.GetStateForSite(actor, site).Activated.OrderByDescending(h => h.Layer.PositionNumber).First();
                    TakeActionState takeActionState = null;

                    if (actor.Prototype.Type == 1)
                    {
                        takeActionState = new TakeActionState(VariableNames.Wealth, site.BiomassValue * (heuristic.GetConsequentValue() / 100));

                        site.BiomassValue -= takeActionState.Value;

                        if (site.BiomassValue <= 0)
                        {
                            currentPeriod.IsOverconsumption = true;
                        }
                    }
                    else
                    {
                        takeActionState = new TakeActionState(heuristic.Consequent.Param, heuristic.GetConsequentValue());

                        heuristic.Apply(actor);
                    }

                    heuristic.FreshnessStatus = 0;

                    currentPeriod.GetStateForSite(actor, site).TakeActions.Add(takeActionState);
                }
            }

            if (actor.Prototype.Type == 1)
            {
                actor[VariableNames.Wealth] = currentPeriod.GetStateForActor(actor).Sum(ss => ss.TakeActions.Sum(ta => ta.Value));
            }
        }
    }
}
