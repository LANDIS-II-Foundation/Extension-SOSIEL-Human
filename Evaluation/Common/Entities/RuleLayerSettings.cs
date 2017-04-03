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
        public string ConsequentRelationshipSign { get; set; }

        public ConsequentRelationship ConsequentRelationship
        {
            get
            {
                switch (ConsequentRelationshipSign)
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
        }

        public int MinValue
        {
            get
            {
                return ConsequentValueInterval[0];
            }
        }

        public int MaxValue
        {
            get
            {
                return ConsequentValueInterval[1];
            }
        }
    }
}
