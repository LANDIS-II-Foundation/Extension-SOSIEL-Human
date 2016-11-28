using System.Collections.Generic;

namespace SocialHuman.Actors
{
    using Entities;
    using Models;
    using Enums;
    using Helpers;
    using Steps;
    using Steps.Abstract;

    class ForestryEnterprise : Actor
    {
        static int indexer = 1;

        public new string Name { get { return ClassName + indexer; } }

        AnticipationLearningAbstract anticipationLearning;
        CounterfactualThinkingAbstract counterfactualThinking;
        InductiveReasoning inductiveReasoning;
        HeuristicSelectionAbstract heuristicSelection;
        TakeAction takeAction;


        public ForestryEnterprise(ActorParameters parameters, Site[] sites)
            :base(parameters, sites)
        {
            name = ClassName + indexer++;

            anticipationLearning = (AnticipationLearningAbstract)StepFactory.Create(parameters.GoalTendency, StepNames.AnticipationLearning);
            counterfactualThinking = (CounterfactualThinkingAbstract)StepFactory.Create(parameters.GoalTendency, StepNames.CounterfactualThinking);
            inductiveReasoning = new InductiveReasoning();
            heuristicSelection = (HeuristicSelectionAbstract)StepFactory.Create(parameters.GoalTendency, StepNames.HeuristicSelection);
            takeAction = new TakeAction();
        }

        public override void SimulatePart1(LinkedListNode<PeriodModel> periodModel)
        {
            PeriodModel currentPeriod = periodModel.Value;

            anticipationLearning.Execute(this, periodModel);

            if(currentPeriod.Confidence == false)
            {
                foreach (Site site in AssignedSites.Randomize())
                {
                    foreach (HeuristicSet set in MentalModel)
                    {
                        foreach (HeuristicLayer layer in set.Layers)
                        {
                            bool CTResult = counterfactualThinking.Execute(this, periodModel, site, layer);

                            inductiveReasoning.Execute(CTResult, this, periodModel, site, layer);
                        }
                    }
                }
            }
        }

        public override void SimulatePart2(LinkedListNode<PeriodModel> periodModel)
        {
            PeriodModel currentPeriod = periodModel.Value;

            foreach (Site site in AssignedSites.Randomize())
            {
                foreach (HeuristicSet set in MentalModel)
                {
                    heuristicSelection.Execute(this, periodModel, site, set);
                }
            }
        }

        public override void SimulateTakeActionPart(LinkedListNode<PeriodModel> periodModel)
        {            
            takeAction.Execute(this, periodModel);
        }
    }
}
