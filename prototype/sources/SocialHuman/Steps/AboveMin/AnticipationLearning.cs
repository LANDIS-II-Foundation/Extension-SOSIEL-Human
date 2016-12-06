using System;

namespace SocialHuman.Steps.AboveMin
{
    using Abstract;
    using Enums;
    using Entities;

    sealed class AnticipationLearning: AnticipationLearningAbstract
    {
        #region Constructors
        public AnticipationLearning():base()
        {
            Tendency = TendencyNames.AboveMin;
        }
        #endregion

        #region Override methods
        protected override void SpecificLogic(ActorGoalState currentGoal)
        {
            if (currentGoal.DiffCurrentAndMin <= 0)
            {
                currentGoal.AnticipatedDirection = AnticipatedDirection.Up;
                currentGoal.Confidence = false;
            }
            else
            {
                currentGoal.AnticipatedDirection = AnticipatedDirection.Stay;
                currentGoal.Confidence = true;
            }
        }
        #endregion

    }
}
