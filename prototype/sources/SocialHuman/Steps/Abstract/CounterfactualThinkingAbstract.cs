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
        protected Heuristic[] matchedPriorPeriodHeuristics;
        protected Heuristic activatedPriorPeriodHeuristic;

        protected PeriodModel currentPeriod;

        protected abstract bool SpecificLogic(HeuristicLayer layer);

        public bool Execute(Actor actor, LinkedListNode<PeriodModel> periodModel, Site site, HeuristicLayer layer)
        {
            currentPeriod = periodModel.Value;

            PeriodModel priorPeriod = periodModel.Previous.Value;

            matchedPriorPeriodHeuristics = priorPeriod.GetDataForSite(actor, site).
                Matched.Where(h=>h.Layer == layer).ToArray();

            if(currentPeriod.Confidence == false && matchedPriorPeriodHeuristics.Length >= 2)
            {
                activatedPriorPeriodHeuristic = priorPeriod.GetDataForSite(actor, site).GetActivated(layer);

                return SpecificLogic(layer);
            }

            return true;
        }
    }
}
