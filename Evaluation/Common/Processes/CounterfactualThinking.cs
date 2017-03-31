using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;

    public class CounterfactualThinking : VolatileProcess
    {
        bool? confidence;

        Goal selectedGoal;
        GoalState selectedGoalState;
        //HeuristicLayer layer;
        Dictionary<Rule, Dictionary<Goal, double>> anticipatedInfluence;

        Rule[] matchedRules;
        Rule activatedRule;


        protected override void AboveMin()
        {
            //
            Rule[] rules = anticipatedInfluence.Where(kvp=> matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value >= 0 && ai.Value > selectedGoalState.DiffCurrentAndMin).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(ai => ai.Value).OrderBy(h => h.Key).First().ToArray();

                confidence = rules.Any(ai => !(ai.AssociatedHeuristic == activatedRule || ai.AssociatedHeuristic.IsAction == false));
            }
        }

        protected override void BelowMax()
        {
            AnticipatedInfluence[] result = anticipatedInfluence
                .Where(ai => ai.Value < 0 && Math.Abs(ai.Value) > Math.Abs(selectedGoalState.DiffCurrentAndMin)).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (result.Length == 0)
            {
                confidence = false;
            }
            else
            {
                result = result.GroupBy(ai => ai.Value).OrderBy(h => h.Key).First().ToArray();

                confidence = result.Any(ai => !(ai.AssociatedHeuristic == activatedRule || ai.AssociatedHeuristic.IsAction == false));
            }
        }

        protected override void Maximize()
        {

        }

        public bool? Execute(IConfigurableAgent agent, LinkedListNode<Dictionary<IConfigurableAgent, AgentState>> lastIteration, Goal[] rankedGoals,
            Rule[] matched, RuleLayer layer)
        {
            confidence = null;

            //Period currentPeriod = periodModel.Value;
            Dictionary<IConfigurableAgent, AgentState> priorIteration = lastIteration.Previous.Value;

            selectedGoal = rankedGoals.First(g => layer.Set.AssociatedWith.Contains(g));

            selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

            activatedRule = priorIteration[agent].Activated.First(r => r.Layer == layer);

            anticipatedInfluence = lastIteration.Value[agent].AnticipationInfluence;

            matchedRules = matched;

            SpecificLogic(selectedGoal.Tendency);

            return confidence;
        }
    }
}
