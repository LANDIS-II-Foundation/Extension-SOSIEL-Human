using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Enums;

    public sealed class RuleLayerSettings
    {
        public bool Modifiable { get; set; } = false;

        public int MaxNumberOfRules { get; set; } = 10;

        public int[] ConsequentValueInterval { get; set; }

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

        public string MaxConsequentReference { get; set; }


        public int MinValue(IAgent agent)
        {
            if(string.IsNullOrEmpty(MinConsequentReference) == false)
            {
                return (int)agent[MinConsequentReference];
            }
            else
            {
                return ConsequentValueInterval[0];
            }
        }

        public int MaxValue(IAgent agent)
        {
            if (string.IsNullOrEmpty(MaxConsequentReference) == false)
            {
                return (int)agent[MaxConsequentReference];
            }
            else
            {
                return ConsequentValueInterval[1];
            }
        }

        public string PreliminaryСalculations { get; set; }
    }
}
