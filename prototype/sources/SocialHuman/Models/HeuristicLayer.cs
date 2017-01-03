using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    public class HeuristicLayer
    {
        #region Private fields
        int heuristicIndexer = 0;
        #endregion

        #region Public fields
        public HeuristicSet Set { get; set; }
        public HeuristicLayerParameters LayerParameters { get; private set; }
        public int PositionNumber { get; set; }
        public List<Heuristic> Heuristics { get; private set; }
        #endregion

        #region Constructors
        public HeuristicLayer(HeuristicLayerParameters parameters)
        {
            Heuristics = new List<Heuristic>(parameters.MaxHeuristicCount);
            LayerParameters = parameters;
        }
        #endregion

        #region Private methods
        void CheckAndRemove()
        {
            if (Heuristics.Count == LayerParameters.MaxHeuristicCount)
            {
                Heuristic oldestHeuristics = Heuristics.OrderByDescending(h => h.FreshnessStatus).First(h => h.IsAction == true);

                Set.UnassignHeuristic(oldestHeuristics);

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
        public void AddRange(IEnumerable<Heuristic> heuristics)
        {
            foreach(Heuristic heuristic in heuristics)
            {
                Add(heuristic);
            }
        }

        #endregion

        
    }
}
