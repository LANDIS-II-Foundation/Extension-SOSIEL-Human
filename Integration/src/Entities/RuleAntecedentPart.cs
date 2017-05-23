using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Environments;
    using Helpers;

    public class RuleAntecedentPart : ICloneable<RuleAntecedentPart>
    {
        

        private Func<dynamic, dynamic, dynamic> antecedent;

        public string Param { get; private set; }

        public string Sign { get; private set; }

        public dynamic Value { get; private set; }

        public string ReferenceVariable { get; private set; }


        public RuleAntecedentPart(string param, string sign, dynamic value, string referenceVariable = null)
        {
            Param = param;
            Sign = sign;
            Value = value;
            ReferenceVariable = referenceVariable;
        }

        /// <summary>
        /// Creates expression tree for condition checking
        /// </summary>
        private void BuildAntecedent()
        {
            antecedent = AntecedentBuilder.Build(Sign);
        }

        /// <summary>
        /// Checks agent variables on antecedent part condition
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates shallow object copy 
        /// </summary>
        /// <returns></returns>
        public RuleAntecedentPart Clone()
        {
            return (RuleAntecedentPart)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of antecedent part but replaces antecedent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newConst"></param>
        /// <returns></returns>
        public static RuleAntecedentPart Renew(RuleAntecedentPart old, dynamic newConst)
        {
            RuleAntecedentPart newAntecedent = old.Clone();

            newAntecedent.antecedent = null;

            newAntecedent.Value = newConst;

            return newAntecedent;
        }
    }
}
