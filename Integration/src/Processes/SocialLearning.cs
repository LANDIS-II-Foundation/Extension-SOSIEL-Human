using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Processes
{
    using Entities;
    using Helpers;


    public class SocialLearning
    {
        Dictionary<IAgent, Dictionary<Goal, List<Rule>>> confidentAgents = new Dictionary<IAgent, Dictionary<Goal, List<Rule>>>();

        public void ExecuteSelection(IAgent agent, AgentState agentState, AgentState previousAgentState,
            RuleLayer layer)
        {
            layer.Set.AssociatedWith.ForEach(goal =>
            {
                GoalState goalState = agentState.GoalsState[goal];

                if (goalState.Confidence == true)
                {
                    Rule activatedRule = previousAgentState.Activated.FirstOrDefault(h => h.Layer == layer);

                    if (confidentAgents.ContainsKey(agent) == false)
                    {
                        confidentAgents.Add(agent, new Dictionary<Goal, List<Rule>>());
                    }

                    if (confidentAgents[agent].ContainsKey(goal) == false)
                    {
                        confidentAgents[agent].Add(goal, new List<Rule>());
                    }

                    confidentAgents[agent][goal].Add(activatedRule);
                }
            });
        }

        public void ExecuteLearning(IAgent[] allAgents, Dictionary<IAgent, AgentState> iterationState, Dictionary<IAgent, Goal[]> rankedGoals)
        {
            allAgents.ForEach(agent =>
            {
                agent.AssignedRules.GroupBy(r => r.Layer).ForEach(layer =>
                {
                    Goal selectedGoal = rankedGoals[agent].First(g => layer.Key.Set.AssociatedWith.Contains(g));

                    agent.ConnectedAgents.ForEach(connectedAgent =>
                    {
                        if (confidentAgents.ContainsKey(connectedAgent) && confidentAgents[connectedAgent].ContainsKey(selectedGoal))
                        {
                            Rule[] availableRules = confidentAgents[connectedAgent][selectedGoal].Where(r => r.Layer == layer.Key).ToArray();


                            availableRules.ForEach(rule =>
                            {
                                agent.AssignNewRule(rule);

                                AgentState agentState = iterationState[agent];

                                if (agentState.AnticipationInfluence.ContainsKey(rule))
                                {
                                    agentState.AnticipationInfluence[rule] = new Dictionary<Goal, double>(iterationState[connectedAgent].AnticipationInfluence[rule]);
                                }
                                else
                                {
                                    agentState.AnticipationInfluence.Add(rule, new Dictionary<Goal, double>(iterationState[connectedAgent].AnticipationInfluence[rule]));
                                }
                            });
                        }
                    });
                });
            });

            //clean state
            confidentAgents.Clear();
        }
    }
}
