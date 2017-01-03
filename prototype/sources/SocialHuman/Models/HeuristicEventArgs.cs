using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Models
{
    public class HeuristicEventArgs : EventArgs
    {
        #region Private fields
        #endregion

        #region Public fields
        public Heuristic TargetHeuristic { get; private set; }
        #endregion

        #region Constructors
        public HeuristicEventArgs(Heuristic heuristic)
        {
            TargetHeuristic = heuristic;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
