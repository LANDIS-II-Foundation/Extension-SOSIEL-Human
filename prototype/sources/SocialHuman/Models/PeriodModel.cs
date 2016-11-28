using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Models
{
    using Actors;
    using Entities;
    using Enums;

    public sealed class PeriodModel
    {
        public int PeriodNumber { get; private set; }
        public Site[] Sites { get; private set; }
        public double TotalBiomass { get { return Sites.Sum(s => s.GoalValue); } }

        //AL
        //may be it's state of actor
        public double AnticipatedInfluence { get; set; }
        public double DiffCurrAndMin { get; set; }
        public bool Confidence { get; set; }
        public AnticipatedDirection AnticipatedDirection { get; set; }

        //TA
        public bool IsOverconsumption { get; set; }

        public List<PeriodPartialModel> PartialData { get; set; }

        
        public PeriodModel(int periodNumber, Site[] sites)
        {
            PeriodNumber = periodNumber;
            Sites = sites;

            PartialData = new List<PeriodPartialModel>();
        }


        /// <summary>
        /// Returns one PartialData for specific actor and site
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public PeriodPartialModel GetDataForSite(Actor actor, Site site)
        {
            return PartialData.Single(pd => pd.Actor == actor && pd.Site.Equals(site));
        }

        /// <summary>
        /// Returns all PartialData for specific actor
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public IEnumerable<PeriodPartialModel> GetDataForActor(Actor actor)
        {
            return PartialData.Where(pd => pd.Actor == actor);
        }

        public Site GetCurrentSiteState(Site site)
        {
            return Sites.Single(s => s.Equals(site));
        }
    }
}
