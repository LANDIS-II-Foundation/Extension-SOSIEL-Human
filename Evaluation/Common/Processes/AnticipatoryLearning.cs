using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;
    using Helpers;

    public class AnticipatoryLearning : VolatileProcess
    {

        GoalState currentGoalState;


        GoalState[] SelectCriticalGoal(IEnumerable<GoalState> goals)
        {
            List<GoalState> tempList = new List<GoalState>();

            IEnumerable<GoalState> noConfidence = goals.Where(gs => gs.Confidence == false).OrderByDescending(gs => Math.Abs(gs.AnticipatedInfluenceValue));

            IEnumerable<GoalState> yesConfidence = goals.Where(gs => gs.Confidence).OrderBy(gs => gs.Goal.Name);

            tempList.AddRange(noConfidence);
            tempList.AddRange(yesConfidence);

            return tempList.ToArray();
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


        public GoalState[] Execute(IConfigurableAgent agent, LinkedListNode<Dictionary<IConfigurableAgent, AgentState>> lastIteration)
        {
            AgentState currentIterationAgentState = lastIteration.Value[agent];
            AgentState previousIterationAgentState = lastIteration.Previous.Value[agent];

            foreach (var kvp in previousIterationAgentState.GoalsState)
            {
                Goal goal = kvp.Key;
                GoalState prevGoalState = kvp.Value;

                currentGoalState = currentIterationAgentState.GoalsState[goal];

                currentGoalState.Value = agent[goal.ReferenceVariable];

                //todo
                //if (goalState.Goal.Increased)
                //    value += goalState.Goal.LimitValue;

                currentGoalState.DiffCurrentAndMin = currentGoalState.Value - currentGoalState.FocalValue;

                //todo: check
                currentGoalState.DiffPriorAndMin = prevGoalState.Value - currentGoalState.FocalValue;

                //goalState.Value contains prior Iteration value
                currentGoalState.AnticipatedInfluenceValue = currentGoalState.Value - prevGoalState.Value;


                //todo
                //currentGS.FocalValue = value;

                //2.Update the anticipated influence of heuristics activated in prior Iteration
                IEnumerable<Rule> activatedInPriorIteration = previousIterationAgentState.Activated;

                //todo
                activatedInPriorIteration.ForEach(r =>
                {
                    currentIterationAgentState.AnticipationInfluence[r][goal] = currentGoalState.AnticipatedInfluenceValue;
                });
                              
                currentGoalState = prevGoalState;

                SpecificLogic(goal.Tendency);


                //todo
                //if (prevGoalState.Goal.Increased)
                //{
                //    prevGoalState.Goal.LimitValue = prevGoalState.FocalValue;
                //}
            }

            return SelectCriticalGoal(agent.AssignedGoals);
        }
    }
}
