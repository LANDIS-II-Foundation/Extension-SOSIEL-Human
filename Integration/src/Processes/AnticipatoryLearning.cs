using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Enums;
    using Entities;
    using Helpers;
    using Exceptions;

    /// <summary>
    /// Anticipatory learning process implementation.
    /// </summary>
    public class AnticipatoryLearning : VolatileProcess
    {

        GoalState currentGoalState;


        /// <summary>
        /// Sorts goals by importance 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="goals"></param>
        /// <returns></returns>
        IEnumerable<Goal> SortByImportance(IAgent agent, Dictionary<Goal, GoalState> goals)
        {
            if (goals.Count > 1)
            {
                var importantGoals = goals.Where(kvp => kvp.Value.Importance > 0).ToArray();

                var noConfidenceGoals = importantGoals.Where(kvp => kvp.Value.Confidence == false).ToArray();


                if (noConfidenceGoals.Length > 0)
                {
                    var noConfidenceProportions = noConfidenceGoals.Select(kvp => new
                    {
                        Proportion = kvp.Value.Importance * (1 + CalculateNormalizedValue(agent, kvp.Key, kvp.Value)),
                        Goal = kvp.Key
                    }).ToArray();

                    double totalNoConfidenctUnadjustedProportions = noConfidenceGoals.Sum(kvp => kvp.Value.Importance);

                    double totalNoConfidenceAdjustedProportions = noConfidenceProportions.Sum(p => p.Proportion);

                    var confidenceGoals = goals.Where(kvp => kvp.Value.Confidence == true).ToArray();

                    var confidenceProportions = confidenceGoals.Select(kvp => new
                    {
                        Proportion = kvp.Value.Importance * (1 - totalNoConfidenceAdjustedProportions) / totalNoConfidenctUnadjustedProportions,
                        Goal = kvp.Key
                    }).ToArray();

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
                    int numberOfInsertions = (int)Math.Round(kvp.Value.AdjustedImportance * 100);

                    for (int i = 0; i < numberOfInsertions; i++) { vector.Add(kvp.Key); }
                });


                for (int i = 0; i < importantGoals.Length && vector.Count > 0; i++)
                {
                    Goal nextGoal = vector.RandomizeOne();

                    vector.RemoveAll(o => o == nextGoal);


                    yield return nextGoal;
                }

                Goal[] otherGoals = goals.Where(kvp => (int)Math.Round(kvp.Value.AdjustedImportance * 100) == 0)
                    .OrderByDescending(kvp => kvp.Key.RankingEnabled).Select(kvp => kvp.Key).ToArray();

                foreach (Goal goal in otherGoals)
                {
                    yield return goal;
                }
            }
            else
            {
                yield return goals.Keys.First();
            }
        }

        /// <summary>
        /// Calculates normalized value for goal prioritizing.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="goal"></param>
        /// <param name="goalState"></param>
        /// <returns></returns>
        double CalculateNormalizedValue(IAgent agent, Goal goal, GoalState goalState)
        {
            double maxPossibleDifference = 0;


            RuleLayerConfiguration layerConfiguration = agent.AssignedRules.Where(rule => rule.Consequent.Param == goal.ReferenceVariable && (rule.Layer.LayerConfiguration.ConsequentValueInterval != null && rule.Layer.LayerConfiguration.ConsequentValueInterval.Length == 2))
                .Select(rule => rule.Layer.LayerConfiguration).FirstOrDefault();

            if(layerConfiguration != null)
            {
                maxPossibleDifference = Math.Max(Math.Abs(layerConfiguration.MaxValue(agent) - goalState.Value), Math.Abs(layerConfiguration.MinValue(agent) - goalState.Value));
            }
            else
            {
                if (goal.Tendency == "EqualToOrAboveFocalValue")
                {
                    maxPossibleDifference = (string.IsNullOrEmpty(goal.FocalValueReference) ? goalState.FocalValue : (double)agent[goal.FocalValueReference]);
                }

                if(goal.Tendency == "EqualToOrBelowFocalValue")
                {
                    double maxValue = agent.AssignedRules.Where(rule => rule.Consequent.Param == goal.ReferenceVariable)
                        .Select(rule => string.IsNullOrEmpty(rule.Consequent.VariableValue) ? (double)rule.Consequent.Value : (double)agent[rule.Consequent.VariableValue])
                        .Max();

                    maxPossibleDifference = maxValue - goalState.PriorValue;
                }
            }

            return Math.Abs(goalState.DiffPriorAndCurrent / maxPossibleDifference);
        }


        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            if (currentGoalState.DiffCurrentAndFocal < 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;

                if (currentGoalState.Value > currentGoalState.PriorValue)
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

        protected override void EqualToOrBelowFocalValue()
        {
            if (currentGoalState.DiffCurrentAndFocal > 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Down;

                if (currentGoalState.Value > currentGoalState.PriorValue)
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
            if (currentGoalState.DiffCurrentAndFocal >= 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                currentGoalState.Confidence = true;
            }
            else
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;
                currentGoalState.Confidence = false;
            }
        }
        #endregion


        /// <summary>
        /// Executes anticipatory learning for specific agent and returns sorted by priority goals array
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <returns></returns>
        public Goal[] Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration)
        {
            AgentState currentIterationAgentState = lastIteration.Value[agent];
            AgentState previousIterationAgentState = lastIteration.Previous.Value[agent];

            foreach (var goal in agent.AssignedGoals)
            {
                GoalState prevGoalState = previousIterationAgentState.GoalsState[goal];

                currentGoalState = currentIterationAgentState.GoalsState[goal];

                currentGoalState.Value = agent[goal.ReferenceVariable];

                if (goal.ChangeFocalValueOnPrevious)
                {
                    double reductionPercent = 1;

                    if (goal.ReductionPercent > 0d)
                        reductionPercent = goal.ReductionPercent;

                    currentGoalState.FocalValue = reductionPercent * previousIterationAgentState.GoalsState[goal].Value;
                }

                double focal = string.IsNullOrEmpty(goal.FocalValueReference) ? currentGoalState.FocalValue : agent[goal.FocalValueReference];

                currentGoalState.DiffCurrentAndFocal = currentGoalState.Value - focal;

                currentGoalState.DiffPriorAndFocal = prevGoalState.Value - focal;

                currentGoalState.DiffPriorAndCurrent = prevGoalState.Value - currentGoalState.Value;

                //goalState.Value contains prior Iteration value
                currentGoalState.AnticipatedInfluenceValue = currentGoalState.Value - prevGoalState.Value;


                //finds activated rules for each site 
                IEnumerable<Rule> activatedInPriorIteration = previousIterationAgentState.RuleHistories.SelectMany(rh => rh.Value.Activated);

                //update anticipated influences of found rules 
                activatedInPriorIteration.ForEach(r =>
                {
                    agent.AnticipationInfluence[r][goal] = currentGoalState.AnticipatedInfluenceValue;
                });

                SpecificLogic(goal.Tendency);
            }

            return SortByImportance(agent, currentIterationAgentState.GoalsState).ToArray();
        }
    }
}
