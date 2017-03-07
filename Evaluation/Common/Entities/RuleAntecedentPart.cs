using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Entities;
    using Helpers;

    public class RuleAntecedentPart : ICloneable
    {
        //public static RuleAntecedentPart Renew(RuleAntecedentPart old, dynamic newConst)
        //{
        //    RuleAntecedentPart newAntecedent = (RuleAntecedentPart)old.Clone();

        //    newAntecedent.antecedent = null;

        //    newAntecedent.Const = newConst;

        //    return newAntecedent;
        //}

        private Func<dynamic, dynamic, dynamic> antecedent;

        public string Param { get; set; }

        public string Sign { get; set; }

        public dynamic Value { get; set; }

        public string ReferenceVariable { get; set; }


        private void BuildAntecedent()
        {
            antecedent = AntecedentBuilder.Build(Sign);
        }

        public bool IsMatch(Agent agent)
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

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
