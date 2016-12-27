using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    using Enums;

    public sealed class HeuristicLayerParameters
    {
        #region Private fields
        #endregion

        #region Public fields
        public bool Modifiable { get; private set; } = true;

        public int MaxHeuristicCount { get; private set; }

        public int[] ConsequentValueInterval { get; private set; }

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
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        
        #endregion

        #region Public methods

        #endregion
    }
}
