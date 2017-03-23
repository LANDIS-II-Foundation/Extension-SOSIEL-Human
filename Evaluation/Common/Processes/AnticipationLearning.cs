using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;

    public class AnticipationLearning : VolatileProcess
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


        public GoalState[] Execute(IConfigurableAgent agent, LinkedListNode<AgentState> lastIteration)
        {
            AgentState currentPeriod = lastIteration.Value;
            AgentState priorPeriod = lastIteration.Previous.Value;

            foreach (GoalState goalState in agent.AssignedGoals)
            {
                double value = agent[goalState.Goal.ReferenceVariable];

                if (goalState.Goal.Increased)
                    value += goalState.Goal.LimitValue;

                goalState.DiffCurrentAndMin = value - goalState.Goal.LimitValue;
                goalState.DiffPriorAndMin = value - goalState.Goal.LimitValue;

                //goalState.Value contains prior period value
                goalState.AnticipatedInfluenceValue = value - goalState.Value;

                goalState.Value = value;

                //2.Update the anticipated influence of heuristics activated in prior period
                IEnumerable<Heuristic> activatedInPriorPeriod = priorPeriod.GetStateForActor(agent).SelectMany(pd => pd.Activated);

                foreach (AnticipatedInfluence ai in agent.AnticipatedInfluences.Where(ai => activatedInPriorPeriod.Contains(ai.AssociatedHeuristic) && ai.AssociatedGoal == goalState.Goal))
                {
                    ai.Value = goalState.AnticipatedInfluenceValue;
                }

                currentGoalState = goalState;

                SpecificLogic(goalState.Goal.Tendency);

                if (goalState.Goal.Increased)
                {
                    goalState.Goal.LimitValue = goalState.Value;
                }
            }

            return SelectCriticalGoal(agent.AssignedGoals);
        }
    }
}
