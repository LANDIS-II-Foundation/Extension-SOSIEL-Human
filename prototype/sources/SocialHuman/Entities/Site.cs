using System;

namespace SocialHuman.Entities
{
    public sealed class Site: ICloneable, IEquatable<Site>
    {
        static int indexer = 1;

        public int Id { get; private set; } = indexer++;
        public double BiomassValue { get; set; }

        public object Clone()
        {
            return new Site { Id = this.Id, BiomassValue = this.BiomassValue };
        }

        public bool Equals(Site other)
        {
            return Id == other.Id;
        }
    }
}
