using System.Linq;

namespace SocialHuman.Steps.AboveMin
{
    using Entities;
    using Steps.Abstract;

    sealed class CounterfactualThinking : CounterfactualThinkingAbstract
    {
        protected override bool SpecificLogic(HeuristicLayer layer)
        {
            double anticipatedInfluence = currentPeriod.AnticipatedInfluence;

            Heuristic resultHeuristic = matchedPriorPeriodHeuristics
                .Where(h => h.AnticipatedInfluence >= 0 && h.AnticipatedInfluence > currentPeriod.DiffCurrAndMin)
                //.Where(h => h.IsMatch(currentPeriod.TotalBiomass))
                .OrderBy(h => h.AnticipatedInfluence).First();

            bool isPriorPeriod = resultHeuristic == activatedPriorPeriodHeuristic;

            //Heuristic[] heuristics = layer.Heuristics.Where(h => h.AnticipatedInfluence >= 0
            //&& h.AnticipatedInfluence > currentPeriod.DiffCurrAndMin && h.AnticipatedInfluence <= minAI).ToArray();

            //not prior period's heuristic OR the “do nothing” heuristic
            return !(isPriorPeriod || resultHeuristic.IsAction == false);
        }
    }
}
