using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Enums;

    public sealed class RuleLayerConfiguration
    {
        public bool Modifiable { get; set; }

        public bool UseDoNothing { get; set; }

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
                    throw new Exception("Unknown consequent relationship. See configuration.");
            }
        }

        public string MinConsequentReference { get; set; }

        public string MaxConsequentReference { get; set; }

        public string PreliminaryСalculations { get; set; }

        public RuleLayerConfiguration()
        {
            Modifiable = false;
        }

        /// <summary>
        /// Gets consequent min value
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets consequent max value
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
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
    }
}
