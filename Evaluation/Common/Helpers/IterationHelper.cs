﻿using System;
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

                    var goalsForRanking = a.Goals.Join(stateConfiguration.AssignedGoals, g => g.Name, gs => gs, (g, gs) => new { g, gs }).ToArray();

                    int numberOfRankingGoals = goalsForRanking.Count(o => o.g.RankingEnabled);

                    goalsForRanking.OrderByDescending(o=>o.g.RankingEnabled).ForEach((o, i) =>
                        {
                            int proportion = unadjustedProportion;

                            if (o.g.RankingEnabled)
                            {
                                if (numberOfRankingGoals > 1 && i < numberOfRankingGoals - 1)
                                {
                                    proportion = LinearUniformRandom.GetInstance.Next(proportion + 1);
                                }

                                unadjustedProportion = unadjustedProportion - proportion;
                            } 
                            else
                            {
                                proportion = 0;
                            }

                            GoalState goalState = new GoalState(0, o.g.FocalValue, proportion * 0.1);

                            agentState.GoalsState.Add(o.g, goalState);
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
