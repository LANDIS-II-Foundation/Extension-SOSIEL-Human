using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Helpers;   

    public class KnowledgeHeuristicsLayer: IComparable<KnowledgeHeuristicsLayer>
    {
        int indexer = 0;
        public int PositionNumber { get; set; }

        public MentalModel Set { get; set; }

        public KnowledgeHeuristicsLayerConfiguration LayerConfiguration { get; private set; }

        public List<KnowledgeHeuristic> KnowledgeHeuristics { get; private set; }

        public KnowledgeHeuristicsLayer(KnowledgeHeuristicsLayerConfiguration configuration)
        {
            KnowledgeHeuristics = new List<KnowledgeHeuristic>(configuration.MaxNumberOfHeuristics);
            LayerConfiguration = configuration;
        }

        public KnowledgeHeuristicsLayer(KnowledgeHeuristicsLayerConfiguration parameters, IEnumerable<KnowledgeHeuristic> heuristics) : this(parameters)
        {
            heuristics.ForEach(r => Add(r));
        }

        /// <summary>
        /// Adds heuristic to the heuristic set layer.
        /// </summary>
        /// <param name="heuristic"></param>
        public void Add(KnowledgeHeuristic heuristic)
        {
            indexer++;
            heuristic.PositionNumber = indexer;
            heuristic.Layer = this;
           
            KnowledgeHeuristics.Add(heuristic);
        }

        /// <summary>
        /// Removes heuristic from heuristic set layer.
        /// </summary>
        /// <param name="heuristic"></param>
        public void Remove(KnowledgeHeuristic heuristic)
        {
            heuristic.Layer = null;

            KnowledgeHeuristics.Remove(heuristic);
        }

        public int CompareTo(KnowledgeHeuristicsLayer other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
