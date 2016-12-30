using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Enums;
    using Models;

    class AnticipationLearning : VolatileStep
    {
        #region Private fields
        GoalState currentGoalState;
        #endregion

        #region Public fields
        #endregion

        #region Private methods
        GoalState[] SelectCriticalGoal(IEnumerable<GoalState> goals)
        {
            return goals.OrderBy(gs => gs.Confidence)
                //todo: may be otherwise 
                .ThenByDescending(gs => Math.Abs(gs.AnticipatedInfluenceValue))
                .ThenBy(gs => gs.Goal.Name).ToArray();
        }
        #endregion


        #region Override methods
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
        #endregion

        #region Public methods
        public GoalState[] Execute(Actor actor, LinkedListNode<Period> periodModel)
        {
            Period currentPeriod = periodModel.Value;
            Period priorPeriod = periodModel.Previous.Value;

            foreach (GoalState goalState in actor.AssignedGoals)
            {
                double value = actor[goalState.Goal.Comment];

                if (goalState.Goal.Increased)
                    value += goalState.Goal.LimitValue;

                goalState.DiffCurrentAndMin = value - goalState.Goal.LimitValue;
                goalState.DiffPriorAndMin = value - goalState.Goal.LimitValue;

                //goalState.Value contains prior period value
                goalState.AnticipatedInfluenceValue = value - goalState.Value;

                goalState.Value = value;

                //2.Update the anticipated influence of heuristics activated in prior period
                IEnumerable<Heuristic> activatedInPriorPeriod = priorPeriod.GetStateForActor(actor).SelectMany(pd => pd.Activated);

                foreach (AnticipatedInfluence ai in actor.AnticipatedInfluences.Where(ai => activatedInPriorPeriod.Contains(ai.AssociatedHeuristic) && ai.AssociatedGoal == goalState.Goal))
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

            return SelectCriticalGoal(actor.AssignedGoals);
        }
        #endregion
    }
}
