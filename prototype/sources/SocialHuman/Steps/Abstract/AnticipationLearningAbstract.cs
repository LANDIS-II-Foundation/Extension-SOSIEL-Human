using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps.Abstract
{
    using Entities;
    using Actors;
    using Models;
    using Randoms;
    

    abstract class AnticipationLearningAbstract
    {
        #region Public fields
        public string Tendency { get; protected set; }
        #endregion

        #region Abstract methods
        protected abstract void SpecificLogic(ActorGoalState currentGoal);
        #endregion

        #region Public methods
        public ActorGoalState Execute(Actor actor, LinkedListNode<Period> periodModel, ActorGoal actorGoal)
        {
            Period currentPeriod = periodModel.Value;
            Period priorPeriod = periodModel.Previous.Value;

            ActorGoalState priorState = priorPeriod.GoalStates[actor].Single(gs => gs.Goal == actorGoal);

            double currentValue;

            //hack
            if (actorGoal.IsPrimary)
            {
                currentValue = priorPeriod.SiteStates[actor].Sum(ss=>ss.TakeActions.Sum(ta => ta.HarvestAmount)) + priorState.Value;
            }
            else
            {
                currentValue = currentPeriod.TotalBiomass;
            }

            

            ActorGoalState currentState = new ActorGoalState(actorGoal, currentValue);

            currentState.DiffCurrentAndMin = currentState.Value - currentState.Goal.MinValue;
            currentState.DiffPriorAndMin = priorState.Value - priorState.Goal.MinValue;

            //1. Calculate anticipated influence(t) == goal variable value(t) – goal variable value(t-1)
            currentState.AnticipatedInfluenceValue = currentState.Value - priorState.Value;

            //2.Update the anticipated influence of heuristics implemented in prior period
            IEnumerable<Heuristic> activatedInPriorPeriod = priorPeriod.GetStateForActor(actor).SelectMany(pd => pd.Activated);

            foreach (Heuristic h in activatedInPriorPeriod)
            {
                AnticipatedInfluence aiForCurrentGoal = h.AnticipatedInfluences.Single(ai => ai.ActorGoal == actorGoal);

                aiForCurrentGoal.Value = currentState.AnticipatedInfluenceValue;
            }

            SpecificLogic(currentState);

            //hack
            if (actorGoal.IsPrimary)
            {
                actorGoal.MinValue = currentState.Value;
            }

            return currentState;
        }

        public void SelectCriticalGoal(List<ActorGoalState> goals)
        {
            ActorGoalState[] confidenceNo = goals.Where(g => g.Confidence == false).ToArray();

            if (confidenceNo.Length > 0)
            {
                if (confidenceNo.Length == 1)
                {
                    confidenceNo[0].IsSelected = true;
                }
                else
                {
                    confidenceNo.OrderByDescending(g=>g.DiffPriorAndMin).First().IsSelected = true;
                }
            }
            else
            {
                goals.Single(g => g.Goal.IsPrimary).IsSelected = true;
            }
        }
        #endregion
    }
}
