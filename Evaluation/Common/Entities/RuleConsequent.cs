using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environments;

    public sealed class RuleConsequent : ICloneable<RuleConsequent>
    {

        public static RuleConsequent Renew(RuleConsequent old, double newValue)
        {
            RuleConsequent newConsequent = old.Clone();

            newConsequent.Value = newValue;

            return newConsequent;
        }


        public string Param { get; set; }

        public dynamic Value { get; set; }

        public string VariableValue { get; set; }

        public bool CopyToCommon { get; set; }

        public bool SavePrevious { get; set; }

        public RuleConsequent Clone()
        {
            return (RuleConsequent)MemberwiseClone();
        }
    }
}
