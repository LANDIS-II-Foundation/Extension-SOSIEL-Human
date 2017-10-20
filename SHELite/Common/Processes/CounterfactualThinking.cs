using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;

    /// <summary>
    /// Counterfactual thinking process implementation.
    /// </summary>
    public class CounterfactualThinking : VolatileProcess
    {
        bool confidence;

        Goal selectedGoal;
        GoalState selectedGoalState;
        //HeuristicLayer layer;
        Dictionary<KnowledgeHeuristic, Dictionary<Goal, double>> anticipatedInfluences;

        KnowledgeHeuristic[] matchedHeuristics;
        KnowledgeHeuristic activatedHeuristic;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            KnowledgeHeuristic[] heuristics = anticipatedInfluences.Where(kvp=> matchedHeuristics.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0 && kvp.Value[selectedGoal] > selectedGoalState.DiffCurrentAndFocal).Select(kvp => kvp.Key).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (heuristics.Length == 0)
            {
                confidence = false;
            }
            else
            {
                heuristics = heuristics.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = heuristics.Any(r => !(r == activatedHeuristic || r.IsAction == false));
            }
        }

        protected override void EqualToOrBelowFocalValue()
        {
            KnowledgeHeuristic[] heuristics = anticipatedInfluences.Where(kvp => matchedHeuristics.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] < 0 && Math.Abs(kvp.Value[selectedGoal]) > Math.Abs(selectedGoalState.DiffCurrentAndFocal)).Select(kvp => kvp.Key).ToArray();

            
            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (heuristics.Length == 0)
            {
                confidence = false;
            }
            else
            {
                heuristics = heuristics.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = heuristics.Any(r => !(r == activatedHeuristic || r.IsAction == false));
            }
        }

        protected override void Maximize()
        {
            KnowledgeHeuristic[] heuristics = anticipatedInfluences.Where(kvp => matchedHeuristics.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0).Select(kvp => kvp.Key).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (heuristics.Length == 0)
            {
                confidence = false;
            }
            else
            {
                heuristics = heuristics.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderByDescending(h => h.Key).First().ToArray();

                confidence = heuristics.Any(r => !(r == activatedHeuristic || r.IsAction == false));
            }
        }

        protected override void Minimize()
        {
            throw new NotImplementedException("Minimize is not implemented in CounterfactualThinking");
        }
        #endregion


        /// <summary>
        /// Executes counterfactual thinking about most important agent goal for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="goal"></param>
        /// <param name="matched"></param>
        /// <param name="layer"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Goal goal,
            KnowledgeHeuristic[] matched, KnowledgeHeuristicsLayer layer, Site site)
        {
            confidence = false;

            //Period currentPeriod = periodModel.Value;
            AgentState priorIterationAgentState = lastIteration.Previous.Value[agent];

            selectedGoal = goal;

            selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

            KnowledgeHeuristicsHistory history = priorIterationAgentState.HeuristicHistories[site];


            activatedHeuristic = history.Activated.First(r => r.Layer == layer);

            anticipatedInfluences = agent.AnticipationInfluence;

            matchedHeuristics = matched;

            SpecificLogic(selectedGoal.Tendency);


            return confidence;
        }
    }
}
