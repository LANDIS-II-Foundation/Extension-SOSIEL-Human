using System;

namespace SocialHuman.Entities
{
    public sealed class Site: ICloneable, IEquatable<Site>
    {
        static int indexer = 1;

        public int Id { get; private set; } = indexer++;
        public double GoalValue { get; set; }

        public object Clone()
        {
            return new Site { Id = this.Id, GoalValue = this.GoalValue };
        }

        public bool Equals(Site other)
        {
            return Id == other.Id;
        }
    }
}
