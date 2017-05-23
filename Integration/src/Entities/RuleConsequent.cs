using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Environments;

    public sealed class RuleConsequent : ICloneable<RuleConsequent>
    {
        public string Param { get; private set; }

        public dynamic Value { get; private set; }

        public string VariableValue { get; private set; }

        public bool CopyToCommon { get; private set; }

        public bool SavePrevious { get; private set; }

        public RuleConsequent(string param, dynamic value, string variableValue = null, bool copyToCommon = false, bool savePrevious = false)
        {
            Param = param;
            Value = value;
            VariableValue = variableValue;
            CopyToCommon = copyToCommon;
            SavePrevious = savePrevious;
        }


        /// <summary>
        /// Creates shallow object copy 
        /// </summary>
        /// <returns></returns>
        public RuleConsequent Clone()
        {
            return (RuleConsequent)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of consequent but replaces consequent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static RuleConsequent Renew(RuleConsequent old, dynamic newValue)
        {
            RuleConsequent newConsequent = old.Clone();

            newConsequent.Value = newValue;

            return newConsequent;
        }
    }
}
