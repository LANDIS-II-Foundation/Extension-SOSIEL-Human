using System.Linq;

namespace SocialHuman.Steps.AboveMin
{
    using Abstract;
    using Entities;
    using Models;
    using Randoms;

    sealed class HeuristicSelection : HeuristicSelectionAbstract
    {
        protected override Heuristic ChooseOne(HeuristicLayer layer, Heuristic[] matchedHeuristics)
        {
            if (currentPeriod.DiffCurrAndMin > 0)
                return priorPeriod.GetDataForSite(actor, site).GetActivated(layer);
            else
            {
                Heuristic[] filteredHeuristics = matchedHeuristics.Where(h => h.AnticipatedInfluence >= 0 &&
                    h.AnticipatedInfluence > currentPeriod.DiffCurrAndMin)
                    .GroupBy(h => h.AnticipatedInfluence).OrderBy(hg => hg.Key).First().ToArray();

                if (filteredHeuristics.Length == 1)
                    return filteredHeuristics[0];
                else
                {
                    return filteredHeuristics[LinearUniformRandom.GetInstance.Next(filteredHeuristics.Length)];
                }
            }
        }
    }
}
