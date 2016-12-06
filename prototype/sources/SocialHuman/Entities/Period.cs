using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Entities
{
    using Actors;

    public sealed class Period
    {
        #region Public fields
        public int PeriodNumber { get; private set; }
        public Site[] Sites { get; private set; }
        public double TotalBiomass { get { return Sites.Sum(s => s.BiomassValue); } }

        public Dictionary<Actor, List<ActorGoalState>> GoalStates { get; private set; } = new Dictionary<Actor, List<ActorGoalState>>();
        public Dictionary<Actor, List<SiteState>> SiteStates { get; private set; } = new Dictionary<Actor, List<SiteState>>();

        //public Dictionary<Actor, List<TakeActionState>> TakeActions { get; private set; } = new Dictionary<Actor, List<TakeActionState>>();

        public bool IsOverconsumption { get; set; }
        #endregion

        #region Constructors
        public Period(int periodNumber, Site[] sites)
        {
            PeriodNumber = periodNumber;
            Sites = sites;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns all SiteState for specific actor
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public IEnumerable<SiteState> GetStateForActor(Actor actor)
        {
            return SiteStates[actor];
        }

        /// <summary>
        /// Returns one SiteState for specific actor and site
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public SiteState GetStateForSite(Actor actor, Site site)
        {
            return SiteStates[actor].Single(ss=> ss.Site.Equals(site));
        }


        /// <summary>
        /// Returns actor's critical goal for current period
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public ActorGoalState GetCriticalGoal(Actor actor)
        {
            return GoalStates[actor].Single(gs => gs.IsSelected);
        }


        /// <summary>
        /// Returns actual value of site biomass
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public Site GetCurrentSiteState(Site site)
        {
            return Sites.Single(s => s.Equals(site));
        }
        #endregion
    }
}
