using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environments;
    using Helpers;

    public class KnowledgeHeuristicAntecedentPart : ICloneable<KnowledgeHeuristicAntecedentPart>, IEquatable<KnowledgeHeuristicAntecedentPart>
    {


        private Func<dynamic, dynamic, dynamic> antecedent;

        public string Param { get; private set; }

        public string Sign { get; private set; }

        public dynamic Value { get; private set; }

        public string ReferenceVariable { get; private set; }


        public KnowledgeHeuristicAntecedentPart(string param, string sign, dynamic value, string referenceVariable = null)
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
        public KnowledgeHeuristicAntecedentPart Clone()
        {
            return (KnowledgeHeuristicAntecedentPart)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of antecedent part but replaces antecedent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newConst"></param>
        /// <returns></returns>
        public static KnowledgeHeuristicAntecedentPart Renew(KnowledgeHeuristicAntecedentPart old, dynamic newConst)
        {
            KnowledgeHeuristicAntecedentPart newAntecedent = old.Clone();

            newAntecedent.antecedent = null;

            newAntecedent.Value = newConst;

            return newAntecedent;
        }

        /// <summary>
        /// Compares two KnowledgeHeuristicAntecedentPart objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(KnowledgeHeuristicAntecedentPart other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other) 
                || (other != null && Param == other.Param && Sign == other.Sign && Value == other.Value && ReferenceVariable == other.ReferenceVariable);
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as KnowledgeHeuristicAntecedentPart);
        }

        public override int GetHashCode()
        {
            //disable comparing by hash code
            return 0;
        }

        public static bool operator ==(KnowledgeHeuristicAntecedentPart a, KnowledgeHeuristicAntecedentPart b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(KnowledgeHeuristicAntecedentPart a, KnowledgeHeuristicAntecedentPart b)
        {
            return !(a == b);
        }
    }
}
