﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environments;

    public sealed class KnowledgeHeuristicConsequent : ICloneable<KnowledgeHeuristicConsequent>, IEquatable<KnowledgeHeuristicConsequent>
    {
        public string Param { get; private set; }

        public dynamic Value { get; private set; }

        public string VariableValue { get; private set; }

        public bool CopyToCommon { get; private set; }

        public bool SavePrevious { get; private set; }

        public KnowledgeHeuristicConsequent(string param, dynamic value, string variableValue = null, bool copyToCommon = false, bool savePrevious = false)
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
        public KnowledgeHeuristicConsequent Clone()
        {
            return (KnowledgeHeuristicConsequent)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of consequent but replaces consequent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static KnowledgeHeuristicConsequent Renew(KnowledgeHeuristicConsequent old, dynamic newValue)
        {
            KnowledgeHeuristicConsequent newConsequent = old.Clone();

            newConsequent.Value = newValue;
            newConsequent.VariableValue = null;

            return newConsequent;
        }


        /// <summary>
        /// Compares two KnowledgeHeuristicConsequent objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(KnowledgeHeuristicConsequent other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other)
                   || (other != null
                       && Param == other.Param
                       && Value == other.Value
                       && VariableValue == other.VariableValue);
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as KnowledgeHeuristicConsequent);
        }

        public override int GetHashCode()
        {
            //disable comparing by hash code
            return 0;
        }

        public static bool operator ==(KnowledgeHeuristicConsequent a, KnowledgeHeuristicConsequent b)
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

        public static bool operator !=(KnowledgeHeuristicConsequent a, KnowledgeHeuristicConsequent b)
        {
            return !(a == b);
        }
    }
}
