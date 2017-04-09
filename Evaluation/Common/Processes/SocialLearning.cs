using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Processes
{
    using Enums;
    using Entities;
    using Models;
    using Helpers;


    public class SocialLearning
    {
        Dictionary<IAgent, List<Rule>> confidentAgents = new Dictionary<IAgent, List<Rule>>();

        public void ExecuteSelection(IAgent agent, AgentState agentState,
            Goal[] rankedGoals, RuleLayer layer)
        {
            GoalState associatedGoalState = agentState.GoalsState[rankedGoals.First(g => layer.Set.AssociatedWith.Contains(g))];


            if (associatedGoalState.Confidence)
            {
                Rule activatedRule = agentState.Activated.Single(h => h.Layer == layer);

                if (confidentAgents.ContainsKey(agent))
                    confidentAgents[agent].Add(activatedRule);
                else
                    confidentAgents.Add(agent, new List<Rule>() { activatedRule });
            }
        }

        public void ExecuteLearning(IAgent[] allAgents, Dictionary<IAgent, AgentState> iterationState)
        {
            foreach (var agent in allAgents)
            {
                foreach (IAgent connectedAgent in agent.ConnectedAgents)
                {
                    foreach (Rule rule in confidentAgents[connectedAgent]
                        .Where(r => r.Layer.Set.AssociatedWith.Any(g => iterationState[agent].GoalsState.Any(kvp => kvp.Key.Name == g.Name))))
                    {
                        if (agent.AssignedRules.Any(r => r != rule))
                        {
                            agent.AssignedRules.Add(rule);

                            AgentState agentState = iterationState[agent];

                            agentState.AnticipationInfluence.Add(rule, new Dictionary<Goal, double>(iterationState[connectedAgent].AnticipationInfluence[rule]));
                        }
                    }
                }
            }

            //clean state
            confidentAgents.Clear();
        }
    }
}
