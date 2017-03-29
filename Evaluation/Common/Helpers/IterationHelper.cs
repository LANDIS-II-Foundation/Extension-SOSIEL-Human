using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Helpers
{
    using Configuration;
    using Randoms;
    using Entities;

    public static class IterationHelper
    {


        public static Dictionary<IConfigurableAgent, AgentState> InitilizeBeginningState(InitialStateConfiguration conf, IEnumerable<IConfigurableAgent> agents)
        {
            Dictionary<IConfigurableAgent, AgentState> temp = new Dictionary<IConfigurableAgent, AgentState>();


            agents.ForEach(a =>
            {
                AgentState agentState = AgentState.Create();
                
                Dictionary<Rule, Dictionary<Goal, double>> ai = new Dictionary<Rule, Dictionary<Goal, double>>();

                a.AssignedRules.ForEach(r =>
                {
                    Dictionary<string, double> source;

                    a.InitialState.AnticipatedInfluenceState.TryGetValue(r.Id, out source);

                    Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

                    a.Goals.ForEach(g =>
                    {
                        inner.Add(g, source != null && source.ContainsKey(g.Name) ? source[g.Name] : 0);
                    });

                    ai.Add(r, inner);
                });

                agentState.AnticipationInfluence = ai;

                if (a.GoalsSettings.GenerateProportions)
                {
                    double unadjustedProportion = 1;

                    int numberOfAssignedGoals = a.InitialState.AssignedGoals.Length;

                    a.InitialState.AssignedGoals.ForEach((gn, i) =>
                    {
                        Goal goal = a.Goals.First(g => g.Name == gn);

                        double proportion = unadjustedProportion;

                        if (numberOfAssignedGoals > 1 && i < numberOfAssignedGoals - 1)
                        {
                            proportion = LinearUniformRandom.GetInstance.Next(Convert.ToInt32(Math.Round(proportion*10)) + 1) * 0.1;

                            unadjustedProportion -= proportion;
                        }
                        
                        GoalState goalState = new GoalState(goal.FocalValue, goal.FocalValue, proportion);

                        agentState.GoalsState.Add(goal, goalState);
                    });
                }
                else
                {
                    a.InitialState.GoalState.ForEach(gs =>
                    {
                        Goal goal = a.Goals.First(g => g.Name == gs.Key);

                        GoalState goalState = new GoalState(gs.Value.Value, goal.FocalValue, gs.Value.Proportion);

                        agentState.GoalsState.Add(goal, goalState);
                    });
                }

                temp.Add(a, agentState);
            });

            return temp;
        }

    }
}
