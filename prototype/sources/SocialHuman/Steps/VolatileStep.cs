using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Steps
{
    abstract class VolatileStep
    {
        #region Private fields
        #endregion

        #region Public fields
        #endregion

        #region Constructors
        #endregion

        #region Abstract methods
        protected abstract void AboveMin();
        protected abstract void BelowMax();
        #endregion

        #region Private methods
        protected void SpecificLogic(string tendency)
        {
            switch(tendency)
            {
                case "AboveMin":
                    AboveMin();
                    break;
                case "BelowMax":
                    BelowMax();
                    break;

                default:
                    throw new Exception("Unknown managing of goal");
            }
        }

        #endregion

        #region Public methods
        #endregion
    }
}
