using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Enums;

    public sealed class RuleLayerParameters
    {
        public bool Modifiable { get; private set; } = false;

        //todo
        public int MaxRuleCount { get; private set; } = 10;

        //todo
        public int[] ConsequentValueInterval { get; private set; }

        //todo
        public string ConsequentRelationshipStr { get; private set; }

        public ConsequentRelationship ConsequentRelationship
        {
            get
            {
                switch (ConsequentRelationshipStr)
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
