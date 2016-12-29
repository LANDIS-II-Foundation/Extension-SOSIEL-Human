using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Models
{
    public sealed class HeuristicConsequentPart: ICloneable
    {
        #region Static methods

        internal static HeuristicConsequentPart Renew(HeuristicConsequentPart old, double newValue)
        {
            HeuristicConsequentPart newConsequent = (HeuristicConsequentPart)old.Clone();

            newConsequent.Value = newValue;

            return newConsequent;
        }

        
        #endregion

        #region Private fields
        #endregion

        #region Public fields
        public string Param { get; private set; }

        public double Value { get; set; }

        public string LinkToValue { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
