using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Processes
{
    using Entities;



    public class SocialLearning
    {
        Dictionary<IAgent, List<Rule>> confidentAgents = new Dictionary<IAgent, List<Rule>>();

        public void ExecuteSelection(IAgent agent, AgentState agentState, AgentState previousAgentState,
            RuleLayer layer)
        {
            if (layer.Set.AssociatedWith.All(g=> agentState.GoalsState[g].Confidence == true))
            {
                Rule activatedRule = previousAgentState.Activated.Single(h => h.Layer == layer);

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
                    if(confidentAgents.ContainsKey(connectedAgent))
                    {
                        foreach (Rule rule in confidentAgents[connectedAgent])
                        {
                            if (agent.AssignedRules.Any(r=>r.Layer == rule.Layer) && agent.AssignedRules.Any(r => r != rule))
                            {
                                agent.AssignNewRule(rule);

                                AgentState agentState = iterationState[agent];

                                agentState.AnticipationInfluence.Add(rule, new Dictionary<Goal, double>(iterationState[connectedAgent].AnticipationInfluence[rule]));
                            }
                        }
                    }
                }
            }

            //clean state
            confidentAgents.Clear();
        }
    }
}
