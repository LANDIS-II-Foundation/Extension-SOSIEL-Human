using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    public sealed class Goal: ICloneable, IEquatable<Goal>
    {
        #region Private fields
        #endregion

        #region Public fields
        public bool Increased { get; private set; }

        public bool IsPrimary { get; private set; }

        public string Name { get; private set; }

        public string Comment { get; private set; }

        public string Tendency { get; private set; }

        public double LimitValue { get; set; }


        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public bool Equals(Goal other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals((Goal)obj);
        }
        #endregion
    }
}
