namespace SocialHuman.Steps.AboveMin
{
    using Abstract;
    using Enums;

    sealed class AnticipationLearning: AnticipationLearningAbstract
    {
        protected override void SpecificLogic()
        {
            if (diffCurrAndMin <= 0)
            {
                currentPeriod.AnticipatedDirection = AnticipatedDirection.Up;
                currentPeriod.Confidence = false;
            }
            else
            {
                currentPeriod.AnticipatedDirection = AnticipatedDirection.Stay;
                currentPeriod.Confidence = true;
            }
        }

    }
}
