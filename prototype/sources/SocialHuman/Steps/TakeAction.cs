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

            if (actor.Prototype.Type == 1)
            {
                foreach (Site site in currentPeriod.GetAssignedSites(actor))
                {
                    //take last layer activated heuristic
                    Heuristic heuristic = currentPeriod.GetStateForSite(actor, site).Activated.OrderByDescending(h => h.Layer.PositionNumber).First();

                    TakeActionState takeActionState = new TakeActionState(heuristic.Consequent.Param, site.BiomassValue * (heuristic.Consequent.Value / 100));

                    site.BiomassValue -= takeActionState.Value;

                    if (site.BiomassValue <= 0)
                    {
                        currentPeriod.IsOverconsumption = true;
                    }

                    currentPeriod.GetStateForSite(actor, site).TakeActions.Add(takeActionState);
                }

                actor[VariablesName.Harvested] = currentPeriod.GetStateForActor(actor).Sum(ss => ss.TakeActions.Sum(ta => ta.Value));
            }
        }
    }
}
