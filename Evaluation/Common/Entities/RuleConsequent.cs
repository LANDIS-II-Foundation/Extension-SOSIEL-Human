using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    public sealed class RuleConsequent : ICloneable
    {

        //internal static RuleConsequent Renew(RuleConsequent old, double newValue)
        //{
        //    RuleConsequent newConsequent = (RuleConsequent)old.Clone();

        //    newConsequent.Value = newValue;

        //    return newConsequent;
        //}


        public string Param { get; set; }

        public double Value { get; set; }

        public string VariableValue { get; set; }

        public bool CopyToCommon { get; set; }

        public bool SavePrevious { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
