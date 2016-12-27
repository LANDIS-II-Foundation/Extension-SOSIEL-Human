using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    using Enums;

    public sealed class Period
    {
        #region Public fields
        public int PeriodNumber { get; private set; }
        public Site[] Sites { get; private set; }
        //public double TotalBiomass { get { return Sites.Sum(s => s.BiomassValue); } }

        //public Dictionary<Actor, GoalState> CriticalGoalState { get; private set; }
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
            if (actor.IsSiteSpecific)
                return SiteStates[actor].Single(ss => ss.Site.Equals(site));
            else
                return SiteStates[actor].First();
        }


        /// <summary>
        /// Returns assigned to actor sites
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public Site[] GetAssignedSites(Actor actor)
        {
            Site[] assignedSites = new Site[] { null };

            if (actor.IsSiteSpecific)
            {
                assignedSites = Sites.Zip((bool[])actor[VariablesName.AssignedSites], (s, sf) => new { s, sf })
                    .Where(o => o.sf).Select(o => o.s).ToArray();

                actor[VariablesName.TotalBiomass] = assignedSites.Sum(s => s.BiomassValue);
            }

            return assignedSites;
        }

        ///// <summary>
        ///// Returns actor's critical goal state for current period
        ///// </summary>
        ///// <param name="actor"></param>
        ///// <returns></returns>
        //public GoalState GetCriticalGoalState(Actor actor)
        //{
        //    return CriticalGoalState[actor];
        //}


        ///// <summary>
        ///// Returns actual value of site biomass
        ///// </summary>
        ///// <param name="site"></param>
        ///// <returns></returns>
        //public Site GetCurrentSiteState(Site site)
        //{
        //    return Sites.Single(s => s.Equals(site));
        //}
        #endregion
    }
}
