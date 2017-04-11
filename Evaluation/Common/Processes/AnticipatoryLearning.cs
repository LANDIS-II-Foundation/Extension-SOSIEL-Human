using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;
    using Helpers;
    using Exceptions;

    public class AnticipatoryLearning : VolatileProcess
    {

        GoalState currentGoalState;


        IEnumerable<Goal> SortByProportion(IAgent agent, Dictionary<Goal, GoalState> goals)
        {
            var importantGoals = goals.Where(kvp => kvp.Value.Importance > 0).ToArray();

            var noConfidenceGoals = importantGoals.Where(kvp => kvp.Value.Confidence == false).ToArray();


            if (noConfidenceGoals.Length > 0)
            {
                var noConfidenceProportions = noConfidenceGoals.Select(kvp =>
                    new { Proportion = kvp.Value.Importance * (1 + Math.Abs(kvp.Value.DiffPriorAndCurrent) / (string.IsNullOrEmpty(kvp.Key.MaxGoalValueReference) ? kvp.Key.MaxGoalValue : (double)agent[kvp.Key.MaxGoalValueReference])), Goal = kvp.Key }).ToArray();

                double totalNoConfidenctUnadjustedProportions = noConfidenceGoals.Sum(kvp => kvp.Value.Importance);

                double totalNoConfidenceAdjustedProportions = noConfidenceProportions.Sum(p => p.Proportion);

                var confidenceGoals = goals.Where(kvp => kvp.Value.Confidence == true).ToArray();

                var confidenceProportions = confidenceGoals.Select(kvp =>
                    new { Proportion = kvp.Value.Importance * (1 - totalNoConfidenceAdjustedProportions) / totalNoConfidenctUnadjustedProportions, Goal = kvp.Key }).ToArray();

                Enumerable.Concat(noConfidenceProportions, confidenceProportions).ForEach(p =>
                {
                    goals[p.Goal].AdjustedImportance = p.Proportion;

                });

            }
            else
            {
                goals.ForEach(kvp =>
                {
                    kvp.Value.AdjustedImportance = kvp.Value.Importance;
                });
            }


            List<Goal> vector = new List<Goal>(100);

            goals.ForEach(kvp =>
            {
                int numberOfInsertions = Convert.ToInt32(Math.Round(kvp.Value.AdjustedImportance * 100));

                for (int i = 0; i < numberOfInsertions; i++) { vector.Add(kvp.Key); }
            });


            for (int i = 0; i < importantGoals.Length && vector.Count > 0; i++)
            {
                Goal nextGoal = vector.RandomizeOne();

                vector.RemoveAll(o => o == nextGoal);


                yield return nextGoal;
            }

            Goal[] otherGoals = goals.Where(kvp => kvp.Value.Importance == 0)
                .OrderByDescending(kvp => kvp.Key.RankingEnabled).Select(kvp => kvp.Key).ToArray();

            foreach (Goal goal in otherGoals)
            {
                yield return goal;
            }
        }


        protected override void AboveMin()
        {
            if (currentGoalState.DiffCurrentAndMin <= 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;

                if (currentGoalState.DiffCurrentAndMin > currentGoalState.DiffPriorAndMin)
                {
                    currentGoalState.Confidence = true;
                }
                else
                {
                    currentGoalState.Confidence = false;
                }
            }
            else
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                currentGoalState.Confidence = true;
            }
        }

        protected override void BelowMax()
        {
            if (currentGoalState.DiffCurrentAndMin > 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Down;

                if (currentGoalState.DiffCurrentAndMin > currentGoalState.DiffPriorAndMin)
                {
                    currentGoalState.Confidence = true;
                }
                else
                {
                    currentGoalState.Confidence = false;
                }
            }
            else
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                currentGoalState.Confidence = true;
            }
        }

        protected override void Maximize()
        {
            if (currentGoalState.DiffCurrentAndMax < 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;

                currentGoalState.Confidence = false;
            }
            else if (currentGoalState.DiffCurrentAndMax == 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                currentGoalState.Confidence = true;
            }
            else
            {
                throw new AlgorithmException("Unexpected value: DiffCurrentAndMax");
            }
        }

        public Goal[] Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration)
        {
            AgentState currentIterationAgentState = lastIteration.Value[agent];
            AgentState previousIterationAgentState = lastIteration.Previous.Value[agent];

            foreach (var kvp in previousIterationAgentState.GoalsState)
            {
                Goal goal = kvp.Key;
                GoalState prevGoalState = kvp.Value;

                currentGoalState = currentIterationAgentState.GoalsState[goal];

                currentGoalState.Value = agent[goal.ReferenceVariable];

                if (goal.ChangeFocalValueOnPrevious)
                    currentGoalState.FocalValue = previousIterationAgentState.GoalsState[goal].Value;

                double focal = string.IsNullOrEmpty(goal.FocalValueReference) ? currentGoalState.FocalValue : agent[goal.FocalValueReference];

                currentGoalState.DiffCurrentAndMin = currentGoalState.Value - focal;

                currentGoalState.DiffPriorAndMin = prevGoalState.Value - focal;

                currentGoalState.DiffPriorAndCurrent = prevGoalState.Value - currentGoalState.Value;


                double goalMax = string.IsNullOrEmpty(goal.MaxGoalValueReference) ? goal.MaxGoalValue : agent[goal.MaxGoalValueReference];

                currentGoalState.DiffPriorAndMax = prevGoalState.Value - goalMax;

                currentGoalState.DiffCurrentAndMax = currentGoalState.Value - goalMax;

                //goalState.Value contains prior Iteration value
                currentGoalState.AnticipatedInfluenceValue = currentGoalState.Value - prevGoalState.Value;


                //2.Update the anticipated influence of heuristics activated in prior Iteration
                IEnumerable<Rule> activatedInPriorIteration = previousIterationAgentState.Activated;

                //todo
                activatedInPriorIteration.ForEach(r =>
                {
                    currentIterationAgentState.AnticipationInfluence[r][goal] = currentGoalState.AnticipatedInfluenceValue;
                });

                SpecificLogic(goal.Tendency);
            }

            return SortByProportion(agent, currentIterationAgentState.GoalsState).ToArray();
        }
    }
}
