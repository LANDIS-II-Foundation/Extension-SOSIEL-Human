using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Environments;
    using Helpers;

    public class RuleAntecedentPart : ICloneable<RuleAntecedentPart>
    {
        public static RuleAntecedentPart Renew(RuleAntecedentPart old, dynamic newConst)
        {
            RuleAntecedentPart newAntecedent = old.Clone();

            newAntecedent.antecedent = null;

            newAntecedent.Value = newConst;

            return newAntecedent;
        }

        private Func<dynamic, dynamic, dynamic> antecedent;

        public string Param { get; set; }

        public string Sign { get; set; }

        public dynamic Value { get; set; }

        public string ReferenceVariable { get; set; }


        private void BuildAntecedent()
        {
            antecedent = AntecedentBuilder.Build(Sign);
        }

        public bool IsMatch(IAgent agent)
        {
            if (antecedent == null)
            {
                BuildAntecedent();
            }

            dynamic value = Value;

            if (string.IsNullOrEmpty(ReferenceVariable) == false)
            {
                value = agent[ReferenceVariable];
            }

            return antecedent(agent[Param], value);
        }

        public RuleAntecedentPart Clone()
        {
            return (RuleAntecedentPart)MemberwiseClone();
        }
    }
}
