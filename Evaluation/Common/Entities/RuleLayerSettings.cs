using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Enums;

    public sealed class RuleLayerSettings
    {
        public bool Modifiable { get; set; } = false;

        //todo
        public int MaxNumberOfRules { get; set; } = 10;

        //todo
        public int[] ConsequentValueInterval { get; set; }

        //todo
        public Dictionary<string, string> ConsequentRelationshipSign { get; set; }

        public static ConsequentRelationship ConvertSign(string sign)
        {
            switch (sign)
            {
                case "+":
                    return ConsequentRelationship.Positive;
                case "-":
                    return ConsequentRelationship.Negative;

                default:
                    //todo
                    throw new Exception("trouble with relationship");
            }
        }

        public string MinConsequentReference { get; set; }

        public string MaxConsequentreference { get; set; }


        public int MinValue(IAgent agent)
        {
            if(string.IsNullOrEmpty(MinConsequentReference) == false)
            {
                return agent[MinConsequentReference];
            }
            else
            {
                return ConsequentValueInterval[0];
            }
        }

        public int MaxValue(IAgent agent)
        {
            if (string.IsNullOrEmpty(MaxConsequentreference) == false)
            {
                return agent[MaxConsequentreference];
            }
            else
            {
                return ConsequentValueInterval[1];
            }
        }

        public string PreliminaryСalculations { get; set; }
    }
}
