using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps.Abstract
{
    using Actors;
    using Entities;
    using Models;

    abstract class HeuristicSelectionAbstract
    {
        protected Actor actor;
        protected Site site;
        protected PeriodModel currentPeriod;
        protected PeriodModel priorPeriod;

        protected abstract Heuristic ChooseOne(HeuristicLayer layer, Heuristic[] matchedHeuristics);

        public void Execute(Actor actor, LinkedListNode<PeriodModel> periodModel, Site site, HeuristicSet set)
        {
            this.actor = actor;
            this.site = site;
            currentPeriod = periodModel.Value;
            priorPeriod = periodModel.Previous.Value;

            List<Heuristic> matched = new List<Heuristic>();
            List<Heuristic> activated = new List<Heuristic>();

            double result = currentPeriod.TotalBiomass;

            foreach (HeuristicLayer layer in set.Layers)
            {
                Heuristic[] matchedForLayer = layer.Heuristics.Where(h => h.IsMatch(result)).ToArray();

                Heuristic activatedHeuristic;

                if (matchedForLayer.Length > 1)
                    activatedHeuristic = ChooseOne(layer, matchedForLayer);
                else
                    activatedHeuristic = matchedForLayer[0];

                activatedHeuristic.FreshnessStatus = 0;

                result = activatedHeuristic.ConsequentValue;

                activated.Add(activatedHeuristic);
                matched.AddRange(matchedForLayer);
            }

            PeriodPartialModel newPeriodPartial = PeriodPartialModel.Create(actor, site, matched.ToArray(), activated.ToArray());

            currentPeriod.PartialData.Add(newPeriodPartial);
        }
    }
}
