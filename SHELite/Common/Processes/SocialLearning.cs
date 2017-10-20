using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;
    using Helpers;


    /// <summary>
    /// Social learning process implementation.
    /// </summary>
    public class SocialLearning
    {
        /// <summary>
        /// Executes social learning process of current agent for specific heuristic set layer
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="layer"></param>
        public void ExecuteLearning(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, KnowledgeHeuristicsLayer layer)
        {
            Dictionary<IAgent, AgentState> priorIterationState = lastIteration.Previous.Value;

            agent.ConnectedAgents.Randomize().ForEach(neighbour =>
            {
                IEnumerable<KnowledgeHeuristic> activatedHeuristics = priorIterationState[neighbour].HeuristicHistories
                    .SelectMany(rh => rh.Value.Activated).Where(r => r.Layer == layer);

                activatedHeuristics.ForEach(heuristic =>
                {
                    if (agent.AssignedKnowledgeHeuristics.Contains(heuristic) == false)
                    {
                        agent.AssignNewHeuristic(heuristic, neighbour.AnticipationInfluence[heuristic]);
                    }
                });

            });
        }
    }
}
