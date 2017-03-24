using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Helpers;

    public sealed class AgentState
    {
        public Dictionary<Rule, Dictionary<Goal, double>> AnticipationInfluence { get; set; } 

        public Dictionary<Goal, GoalState> GoalsState { get; set; }

        public List<Rule> Matched { get; private set; }
        public List<Rule> Activated { get; private set; }

        private AgentState() { }

        public static AgentState Create(IEnumerable<Rule> matched, IEnumerable<Rule> activated, bool isSiteSpecific = false, Site site = null)
        {
            AgentState agentState = Create(isSiteSpecific, site);

            agentState.Matched.AddRange(matched);
            agentState.Activated.AddRange(activated);

            return agentState;
        }

        public static AgentState Create(bool isSiteSpecific, Site site = null)
        {
            if (isSiteSpecific && site == null)
                throw new Exception("site can't be null");

            return new AgentState
            {
                //IsSiteSpecific = isSiteSpecific,
                //Site = site,
                Matched = new List<Rule>(),
                Activated = new List<Rule>(),
                AnticipationInfluence = new Dictionary<Rule, Dictionary<Goal, double>>(),
                GoalsState = new Dictionary<Goal, GoalState>()
                //TakeActions = new List<TakeActionState>()
            };
        }

        public AgentState CreateForNextIteration()
        {
            AgentState agentState = Create(false);

            agentState.AnticipationInfluence = new Dictionary<Rule, Dictionary<Goal, double>>(AnticipationInfluence);

            GoalsState.ForEach(kvp =>
            {
                agentState.GoalsState.Add(kvp.Key, kvp.Value.CreateForNextIteration());
            });

            return agentState;
        }
    }
}
