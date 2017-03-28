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
                AgentState agentState = AgentState.Create();
                
                AgentStateConfiguration agentStateConfiguration = conf.AgentsState[a.Id.ToString()];

                Dictionary<Rule, Dictionary<Goal, double>> ai = new Dictionary<Rule, Dictionary<Goal, double>>();

                a.AssignedRules.ForEach(r =>
                {
                    Dictionary<string, double> source;

                    agentStateConfiguration.AnticipatedInfluenceState.TryGetValue(r.Id, out source);

                    Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

                    a.Goals.ForEach(g =>
                    {
                        inner.Add(g, source.ContainsKey(g.Name) ? source[g.Name] : 0);
                    });

                    ai.Add(r, inner);
                });

                agentState.AnticipationInfluence = ai;


                agentStateConfiguration.GoalState.ForEach(gs =>
               {
                   Goal goal = a.Goals.First(g => g.Name == gs.Key);

                   GoalState goalState = new GoalState(gs.Value.Value, goal.FocalValue, gs.Value.Proportion);

                   agentState.GoalsState.Add(goal, goalState);
               });

                temp.Add(a, agentState);
            });

            return temp;
        }

    }
}
