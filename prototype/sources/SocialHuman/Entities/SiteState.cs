using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Entities
{
    public sealed class SiteState
    {
        #region Public fields
        public Site Site { get; private set; }
        //public HeuristicSet HeuristicSet { get; private set; } 
        public Heuristic[] Matched { get; private set; }
        public Heuristic[] Activated { get; private set; }
        public List<TakeActionState> TakeActions { get; private set; }
        #endregion

        #region Constructors
        private SiteState() { }
        #endregion

        #region Factory methods
        public static SiteState Create(Site site, Heuristic[] matched, Heuristic[] activated)
        {
            return new SiteState
            {
                Site = site,
                Matched = matched,
                Activated = activated,
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heuristicSet"></param>
        /// <returns>Returns TakeActionState for specific heuristic set</returns>
        public TakeActionState GetTakeActionForSet(HeuristicSet heuristicSet)
        {
            return TakeActions.Single(ta => ta.Set == heuristicSet);
        }
        #endregion
    }
}
