using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Models
{
    using Builders;
    public sealed class HeuristicAntecedentPart: ICloneable
    {
        #region Static methods
        internal static HeuristicAntecedentPart Renew(HeuristicAntecedentPart old, dynamic newConst)
        {
            HeuristicAntecedentPart newAntecedent = (HeuristicAntecedentPart)old.Clone();

            newAntecedent.antecedent = null;

            newAntecedent.Const = newConst;

            return newAntecedent;
        }
        
        #endregion

        #region Private fields
        private Func<dynamic, dynamic, dynamic> antecedent;
        #endregion

        #region Public fields
        public bool Immutable { get; private set; } = true;

        public string Param { get; private set; }

        public string Sign { get; private set; }

        public dynamic Const { get; set; }

        public string LinkForConst { get; private set; }
        #endregion

        #region Constructors

        #endregion

        #region Private methods
        private void BuildAntecedent()
        {
            antecedent = AntecedentBuilder.Build(Sign);
        }
        #endregion

        #region Public methods
        public bool IsMatch(dynamic value)
        {
            if (antecedent == null)
            {
                BuildAntecedent();
            }

            return antecedent(value, Const);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
