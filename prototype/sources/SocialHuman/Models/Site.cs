using System;

namespace SocialHuman.Models
{
    public sealed class Site: ICloneable, IEquatable<Site>
    {
        static int indexer = 1;

        public int Id { get; private set; } = indexer++;
        public double BiomassValue { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Equals(Site other)
        {
            return Id == other.Id;
        }
    }
}
