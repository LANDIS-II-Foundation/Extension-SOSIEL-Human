using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Entities
{
    using Models;

    public class HeuristicLayer
    {
        readonly int maxHeuristic;

        int heuristicIndexer = 0;
        public HeuristicSet Set { get; set; }
        public HeuristicConsequentRule ConsequentRule { get; private set; }

        public int PositionNumber { get; set; }
        
        public List<Heuristic> Heuristics { get; private set; }

        public HeuristicLayer(int maxHeuristic, HeuristicConsequentRule rule)
        {
            this.maxHeuristic = maxHeuristic;
            Heuristics = new List<Heuristic>(maxHeuristic);
            ConsequentRule = rule;
        }

        public void Add(Heuristic heuristic)
        {
            heuristicIndexer++;
            heuristic.PositionNumber = heuristicIndexer;
            heuristic.Layer = this;

            Heuristics.Add(heuristic);

            //todo: add remove logic
        }

        void CheckAndRemove()
        {
            if(Heuristics.Count == maxHeuristic)
            {
                Heuristic oldestHeuristics = Heuristics.OrderByDescending(h => h.FreshnessStatus).First(h => h.IsAction == true);

                Heuristics.Remove(oldestHeuristics);
            }
        }
    }
}
