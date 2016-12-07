using System;
using System.Collections.Generic;
using System.Linq;

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
        #region Private fields
        //static int indexer = 1;

        AnticipationLearningAbstract anticipationLearning;
        CounterfactualThinkingAbstract counterfactualThinking;
        InductiveReasoning inductiveReasoning;
        HeuristicSelectionAbstract heuristicSelection;
        TakeAction takeAction;
        #endregion


        #region Constructors
        public ForestryEnterprise(ActorParameters parameters, Site[] sites)
            : base(parameters, sites)
        {
            ActorGoal firstGoal = parameters.Goals.First();

            anticipationLearning = (AnticipationLearningAbstract)StepFactory.Create(firstGoal.Tendency, StepNames.AnticipationLearning);

            counterfactualThinking = (CounterfactualThinkingAbstract)StepFactory.Create(firstGoal.Tendency, StepNames.CounterfactualThinking);
            inductiveReasoning = new InductiveReasoning();
            heuristicSelection = (HeuristicSelectionAbstract)StepFactory.Create(firstGoal.Tendency, StepNames.HeuristicSelection);
            takeAction = new TakeAction();
        }
        #endregion

        #region Public methods
        public override void SimulatePart1(LinkedListNode<Period> periodModel)
        {
            Period currentPeriod = periodModel.Value, priorPeriod = periodModel.Previous.Value;

            currentPeriod.SiteStates.Add(this, new List<SiteState>(AssignedSites.Count));

            List<ActorGoalState> goalStatesList = new List<ActorGoalState>(Goals.Length);

            foreach (ActorGoal goal in Goals)
            {
                if (anticipationLearning.Tendency != goal.Tendency)
                {
                    anticipationLearning = (AnticipationLearningAbstract)StepFactory.Create(goal.Tendency, StepNames.AnticipationLearning);
                }

                ActorGoalState currentGoal = anticipationLearning.Execute(this, periodModel, goal);
                goalStatesList.Add(currentGoal);
            }
            anticipationLearning.SelectCriticalGoal(goalStatesList);

            currentPeriod.GoalStates.Add(this, goalStatesList);


            ActorGoalState criticalGoal = currentPeriod.GetCriticalGoal(this);

            //todo: change if possible
            if (counterfactualThinking.Tendency != criticalGoal.Goal.Tendency)
            {
                counterfactualThinking = (CounterfactualThinkingAbstract)StepFactory.Create(criticalGoal.Goal.Tendency, StepNames.CounterfactualThinking);
                heuristicSelection = (HeuristicSelectionAbstract)StepFactory.Create(criticalGoal.Goal.Tendency, StepNames.HeuristicSelection);
            }


            if (criticalGoal.Confidence == false)
            {
                foreach (Site site in AssignedSites.Randomize())
                {
                    foreach (HeuristicSet set in MentalModel)
                    {
                        foreach (HeuristicLayer layer in set.Layers)
                        {
                            Heuristic[] matchedPriorPeriodHeuristics = priorPeriod.GetStateForSite(this, site).
                                Matched.Where(h => h.Layer == layer).ToArray();

                            bool CTResult = true;

                            if (matchedPriorPeriodHeuristics.Length >= 2)
                                CTResult = counterfactualThinking.Execute(this, periodModel, site, layer);

                            if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
                                inductiveReasoning.Execute(this, periodModel, site, layer);
                        }
                    }
                }
            }
        }

        public override void SimulatePart2(LinkedListNode<Period> periodModel)
        {
            Period currentPeriod = periodModel.Value;

            foreach (Site site in AssignedSites.Randomize())
            {
                foreach (HeuristicSet set in MentalModel)
                {
                    heuristicSelection.Execute(this, periodModel, site, set);
                }
            }
        }

        public override void SimulateTakeActionPart(LinkedListNode<Period> periodModel)
        {
            takeAction.Execute(this, periodModel);
        }
        #endregion

    }
}
