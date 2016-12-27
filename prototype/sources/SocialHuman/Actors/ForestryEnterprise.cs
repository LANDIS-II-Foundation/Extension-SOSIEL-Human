using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Actors
{
    using Models;
    using Enums;
    using Helpers;
    using Steps;
    using Steps.Abstract;


    class ForestryEnterprise : Actor
    {
        #region Private fields
        //static int indexer = 1;

        AnticipationLearning anticipationLearning;
        CounterfactualThinking counterfactualThinking;
        InductiveReasoning inductiveReasoning;
        HeuristicSelection heuristicSelection;
        TakeAction takeAction;
        #endregion


        #region Constructors
        public ForestryEnterprise(ActorParameters parameters, Site[] sites)
            : base(parameters, sites)
        {
            ActorGoal firstGoal = parameters.Goals.First();

            anticipationLearning = (AnticipationLearning)StepFactory.Create(firstGoal.Tendency, StepNames.AnticipationLearning);

            counterfactualThinking = (CounterfactualThinking)StepFactory.Create(firstGoal.Tendency, StepNames.CounterfactualThinking);
            inductiveReasoning = new InductiveReasoning();
            heuristicSelection = (HeuristicSelection)StepFactory.Create(firstGoal.Tendency, StepNames.HeuristicSelection);
            takeAction = new TakeAction();
        }
        #endregion

        #region Public methods
        public override void SimulatePart1(LinkedListNode<Period> periodModel)
        {
            
        }

        public override void SimulatePart2(LinkedListNode<Period> periodModel)
        {
            
        }

        public override void SimulateTakeActionPart(LinkedListNode<Period> periodModel)
        {
            
        }
        #endregion

    }
}
