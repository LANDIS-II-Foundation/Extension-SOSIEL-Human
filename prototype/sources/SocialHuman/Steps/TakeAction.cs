using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Actors;
    using Entities;
    using Models;

    sealed class TakeAction
    {
        public void Execute(Actor actor, LinkedListNode<PeriodModel> periodModel)
        {
            PeriodModel currentPeriod = periodModel.Value;

            foreach (Site site in currentPeriod.Sites)
            {
                if (actor.IsSiteAssigned(site))
                {
                    Heuristic[] activatedHeuristics = currentPeriod.GetDataForSite(actor, site).Activated;

                    foreach (var set in activatedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                    {
                        double consequentValue = set.OrderBy(h => h.Layer.PositionNumber).Last().ConsequentValue;

                        site.GoalValue -= site.GoalValue * consequentValue;

                        if(site.GoalValue <= 0)
                        {
                            currentPeriod.IsOverconsumption = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}
