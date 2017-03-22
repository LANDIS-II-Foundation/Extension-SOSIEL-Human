using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    public sealed class AgentState
    {



        public List<Rule> Matched { get; private set; }
        public List<Rule> Activated { get; private set; }

        private AgentState() { }
        
        //public static AgentState Create(bool isSiteSpecific, Heuristic[] matched, Heuristic[] activated, Site site = null)
        //{
        //    SiteState siteState = Create(isSiteSpecific, site);

        //    siteState.Matched.AddRange(matched);
        //    siteState.Activated.AddRange(activated);

        //    return siteState;
        //}

        //public static AgentState Create(bool isSiteSpecific, Site site = null)
        //{
        //    if (isSiteSpecific && site == null)
        //        throw new Exception("site can't be null");

        //    return new AgentState
        //    {
        //        IsSiteSpecific = isSiteSpecific,
        //        Site = site,
        //        Matched = new List<Heuristic>(),
        //        Activated = new List<Heuristic>(),
        //        TakeActions = new List<TakeActionState>()
        //    };
        //}


    }
}
