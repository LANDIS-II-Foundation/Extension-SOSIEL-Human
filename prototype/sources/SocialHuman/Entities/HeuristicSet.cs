using System.Collections.Generic;

namespace SocialHuman.Entities
{
    public class HeuristicSet
    {
        int layerIndexer = 0;

        public int PositionNumber { get; set; }
        public List<HeuristicLayer> Layers { get; set; } = new List<HeuristicLayer>();

        public HeuristicSet(int number)
        {
            PositionNumber = number;
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
