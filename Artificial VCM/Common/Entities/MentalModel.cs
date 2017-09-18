using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Entities
{
    using Helpers;


    public class MentalModel: IComparable<MentalModel>
    {
        int layerIndexer = 0;

        public int PositionNumber { get; set; }
        public List<KnowledgeHeuristicsLayer> Layers { get; private set; } 

        public Goal[] AssociatedWith { get; private set; }

        private MentalModel(int number, Goal[] associatedGoals)
        {
            PositionNumber = number;
            Layers = new List<KnowledgeHeuristicsLayer>();
            AssociatedWith = associatedGoals;
        }

        public MentalModel(int number, Goal[] associatedGoals, IEnumerable<KnowledgeHeuristicsLayer> layers) :this(number, associatedGoals)
        {
            layers.ForEach(l => Add(l));
        }

        /// <summary>
        /// Adds layer to the heuristic set.
        /// </summary>
        /// <param name="layer"></param>
        public void Add(KnowledgeHeuristicsLayer layer)
        {
            layerIndexer++;
            layer.Set = this;
            layer.PositionNumber = layerIndexer;

            Layers.Add(layer);
        }


        public IEnumerable<KnowledgeHeuristic> AsHeuristicEnumerable()
        {
            return Layers.SelectMany(rl => rl.KnowledgeHeuristics);
        }

        public int CompareTo(MentalModel other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
