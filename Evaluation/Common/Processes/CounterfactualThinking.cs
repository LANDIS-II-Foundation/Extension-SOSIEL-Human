using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;

    public class CounterfactualThinking : VolatileProcess
    {
        bool confidence;

        Goal selectedGoal;
        GoalState selectedGoalState;
        //HeuristicLayer layer;
        Dictionary<Rule, Dictionary<Goal, double>> anticipatedInfluences;

        Rule[] matchedRules;
        Rule activatedRule;


        protected override void AboveMin()
        {
            Rule[] rules = anticipatedInfluences.Where(kvp=> matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0 && kvp.Value[selectedGoal] > selectedGoalState.DiffCurrentAndMin).Select(kvp => kvp.Key).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = rules.Any(r => !(r == activatedRule || r.IsAction == false));
            }
        }

        protected override void BelowMax()
        {
            Rule[] rules = anticipatedInfluences.Where(kvp => matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] < 0 && Math.Abs(kvp.Value[selectedGoal]) > Math.Abs(selectedGoalState.DiffCurrentAndMin)).Select(kvp => kvp.Key).ToArray();

            
            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = rules.Any(r => !(r == activatedRule || r.IsAction == false));
            }
        }

        protected override void Maximize()
        {
            Rule[] rules = anticipatedInfluences.Where(kvp => matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0).Select(kvp => kvp.Key).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderByDescending(h => h.Key).First().ToArray();

                confidence = rules.Any(r => !(r == activatedRule || r.IsAction == false));
            }
        }

        public bool Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Goal goal,
            Rule[] matched, RuleLayer layer)
        {
            confidence = false;

            //Period currentPeriod = periodModel.Value;
            Dictionary<IAgent, AgentState> priorIteration = lastIteration.Previous.Value;

            selectedGoal = goal;

            selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

            activatedRule = priorIteration[agent].Activated.First(r => r.Layer == layer);

            anticipatedInfluences = lastIteration.Value[agent].AnticipationInfluence;

            matchedRules = matched;

            SpecificLogic(selectedGoal.Tendency);


            return confidence;
        }
    }
}
