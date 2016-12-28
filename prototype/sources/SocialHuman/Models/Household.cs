using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Models
{
    sealed class Household
    {
        #region Private fields
        #endregion

        #region Public fields
        public string Name { get; private set; }

        public double Income { get; set; }

        public double Expenses { get; set; }

        public double Savings
        {
            get
            {
                return Income - Expenses;
            }
        }
        #endregion

        #region Constructors
        public Household(string name)
        {
            Name = name;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
