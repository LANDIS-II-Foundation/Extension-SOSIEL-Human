using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    public sealed class SiteState
    {
        #region Public fields
        public bool IsSiteSpecific { get; private set; }
        public Site Site { get; private set; }
        //public HeuristicSet HeuristicSet { get; private set; } 
        public List<Heuristic> Matched { get; private set; }
        public List<Heuristic> Activated { get; private set; }
        public List<TakeActionState> TakeActions { get; private set; }
        #endregion

        #region Constructors
        private SiteState() { }
        #endregion

        #region Factory methods
        public static SiteState Create(bool isSiteSpecific, Heuristic[] matched, Heuristic[] activated, Site site = null)
        {
            SiteState siteState = Create(isSiteSpecific, site);

            siteState.Matched.AddRange(matched);
            siteState.Activated.AddRange(matched);

            return siteState;
        }

        public static SiteState Create(bool isSiteSpecific, Site site = null)
        {
            if (isSiteSpecific && site == null)
                throw new Exception("site can't be null");

            return new SiteState
            {
                IsSiteSpecific = isSiteSpecific,
                Site = site,
                Matched = new List<Heuristic>(),
                Activated = new List<Heuristic>(),
                TakeActions = new List<TakeActionState>()
            };
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns activated heuristic for specific layer 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Heuristic GetActivated(HeuristicLayer layer)
        {
            return Activated.Single(h => h.Layer == layer);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="heuristicSet"></param>
        ///// <returns>Returns TakeActionState for specific heuristic set</returns>
        //public TakeActionState GetTakeActionForSet(HeuristicSet heuristicSet)
        //{
        //    return TakeActions.Single(ta => ta.Set == heuristicSet);
        //}
        #endregion
    }
}
