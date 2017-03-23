using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Helpers
{
    using Configuration;
    using Entities;

    public static class IterationHelper
    {
        

        public static Dictionary<IConfigurableAgent, AgentState> InitilizeBeginningState(InitialStateConfiguration conf, IEnumerable<IConfigurableAgent> agents)
        {
            Dictionary<IConfigurableAgent, AgentState> temp = new Dictionary<IConfigurableAgent, AgentState>();


            agents.ForEach(a =>
            {
                AgentStateConfiguration agentStateConfiguration = conf.AgentsState[a.Id.ToString()];

                AgentState agentState = AgentState.Create(
                    a.AssignedRules.Where(r => agentStateConfiguration.MatchedRules.Contains(r.Id)),
                    a.AssignedRules.Where(r => agentStateConfiguration.ActivatedRules.Contains(r.Id)));

                Dictionary<Rule, Dictionary<Goal, double>> ai = new Dictionary<Rule, Dictionary<Goal, double>>();

                a.AssignedRules.ForEach(r =>
                {
                    var source = conf.AnticipatedInfluenceState;

                    var inner = a.Goals.Join(source[r.Id], g => g.Name, kvp => kvp.Key, (g, kvp) => new { Key = g, kvp.Value }).ToDictionary(o => o.Key, o => o.Value);

                    ai.Add(r, inner);
                });

                agentState.AnticipationInfluence = ai;


                temp.Add(a, agentState);
            });

            return temp;
        }

    }
}
