using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Entities
{
    using Models;

    public class HeuristicLayer
    {
        #region Private fields
        readonly int maxHeuristic;
        int heuristicIndexer = 0;
        #endregion

        #region Public fields
        public HeuristicSet Set { get; set; }
        public HeuristicConsequentRule ConsequentRule { get; private set; }
        public int PositionNumber { get; set; }
        public List<Heuristic> Heuristics { get; private set; }
        #endregion

        #region Constructors
        public HeuristicLayer(int maxHeuristic, HeuristicConsequentRule rule)
        {
            this.maxHeuristic = maxHeuristic;
            Heuristics = new List<Heuristic>(maxHeuristic);
            ConsequentRule = rule;
        }
        #endregion

        #region Private methods
        void CheckAndRemove()
        {
            if (Heuristics.Count == maxHeuristic)
            {
                Heuristic oldestHeuristics = Heuristics.OrderByDescending(h => h.FreshnessStatus).First(h => h.IsAction == true);

                Heuristics.Remove(oldestHeuristics);
            }
        }
        #endregion


        #region Public methods
        public void Add(Heuristic heuristic)
        {
            CheckAndRemove();

            heuristicIndexer++;
            heuristic.PositionNumber = heuristicIndexer;
            heuristic.Layer = this;

            Heuristics.Add(heuristic);
        }
        #endregion

        
    }
}
