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


        public static Dictionary<IAgent, AgentState> InitilizeBeginningState(InitialStateConfiguration configuration, IEnumerable<IAgent> agents)
        {
            Dictionary<IAgent, AgentState> temp = new Dictionary<IAgent, AgentState>();


            agents.ForEach(a =>
            {
                AgentState agentState = AgentState.Create();

                Dictionary<Rule, Dictionary<Goal, double>> ai = new Dictionary<Rule, Dictionary<Goal, double>>();

                AgentStateConfiguration stateConfiguration = configuration.AgentsState.Single(st => st.PrototypeOfAgent == a.PrototypeName);

                a.AssignedRules.ForEach(r =>
                {
                    Dictionary<string, double> source;

                    stateConfiguration.AnticipatedInfluenceState.TryGetValue(r.Id, out source);

                    Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

                    a.Goals.ForEach(g =>
                    {
                        inner.Add(g, source != null && source.ContainsKey(g.Name) ? source[g.Name] : 0 /*(r.IsAction ? 0 : -1)*/);
                    });

                    ai.Add(r, inner);
                });

                agentState.AnticipationInfluence = ai;

                if (configuration.GenerateGoalProportions)
                {
                    int unadjustedProportion = 10;

                    int numberOfAssignedGoals = stateConfiguration.AssignedGoals.Length;

                    stateConfiguration.AssignedGoals.ForEach((gn, i) =>
                    {
                        Goal goal = a.Goals.First(g => g.Name == gn);

                        int proportion = unadjustedProportion;

                        if (numberOfAssignedGoals > 1 && i < numberOfAssignedGoals - 1)
                        {
                            proportion = LinearUniformRandom.GetInstance.Next(proportion + 1);

                            unadjustedProportion = unadjustedProportion - proportion;
                        }

                        GoalState goalState = new GoalState(0, goal.FocalValue, proportion * 0.1);

                        agentState.GoalsState.Add(goal, goalState);
                    });
                }
                else
                {
                    stateConfiguration.GoalState.ForEach(gs =>
                    {
                        Goal goal = a.Goals.First(g => g.Name == gs.Key);

                        GoalState goalState = new GoalState(0, goal.FocalValue, gs.Value.Proportion);

                        agentState.GoalsState.Add(goal, goalState);
                    });
                }


                Rule[] firstIterationsRule = stateConfiguration.ActivatedRulesOnFirstIteration.Select(rId => a.AssignedRules.First(ar => ar.Id == rId)).ToArray();

                agentState.Matched.AddRange(firstIterationsRule);
                agentState.Activated.AddRange(firstIterationsRule);

                temp.Add(a, agentState);
            });

            return temp;
        }

    }
}
