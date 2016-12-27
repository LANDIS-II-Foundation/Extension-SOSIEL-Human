using System.Collections.Generic;

namespace SocialHuman.Models
{
    public class HeuristicSet
    {
        int layerIndexer = 0;

        public int PositionNumber { get; private set; }
        public List<HeuristicLayer> Layers { get; private set; } = new List<HeuristicLayer>();

        public Goal[] AssociatedWith { get; private set; }

        public HeuristicSet(int number, Goal[] goals)
        {
            PositionNumber = number;
            AssociatedWith = goals;
        }

        public void Add(HeuristicLayer layer)
        {
            layerIndexer++;
            layer.Set = this;
            layer.PositionNumber = layerIndexer;

            Layers.Add(layer);
        }
    }
}
