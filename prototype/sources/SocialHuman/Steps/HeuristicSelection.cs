using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Models;
    using Randoms;


    class HeuristicSelection : VolatileStep
    {


        #region Private fields

        GoalState criticalGoalState;
        AnticipatedInfluence[] aiForMatchedHeuristics;

        Heuristic priorPeriodActivatedHeuristic;
        Heuristic heuristicForActivating;
        #endregion

        #region Override methods
        protected override void AboveMin()
        {
            if (criticalGoalState.DiffCurrentAndMin > 0)
                heuristicForActivating = priorPeriodActivatedHeuristic;
            else
            {
                Goal goal = criticalGoalState.Goal;

                AnticipatedInfluence[] selectedAI = aiForMatchedHeuristics.Where(ai => ai.Value >= 0 &&
                    ai.Value > criticalGoalState.DiffCurrentAndMin).ToArray();

                //if none are identified, then choose the do-nothing heuristic.
                if (selectedAI.Length == 0)
                    heuristicForActivating = aiForMatchedHeuristics.Select(ai => ai.AssociatedHeuristic).Single(h => h.IsAction == false);
                else
                {
                    selectedAI = selectedAI.GroupBy(ai => ai.Value).OrderBy(hg => hg.Key).First().ToArray();

                    if (selectedAI.Length == 1)
                        heuristicForActivating = selectedAI[0].AssociatedHeuristic;
                    else
                    {
                        heuristicForActivating = selectedAI[LinearUniformRandom.GetInstance.Next(selectedAI.Length)].AssociatedHeuristic;
                    }
                }
            }
        }

        protected override void BelowMax()
        {
            if (criticalGoalState.DiffCurrentAndMin < 0)
                heuristicForActivating = priorPeriodActivatedHeuristic;
            else
            {
                Goal goal = criticalGoalState.Goal;

                AnticipatedInfluence[] selectedAI = aiForMatchedHeuristics.Where(ai => ai.Value < 0 &&
                    Math.Abs(ai.Value) > Math.Abs(criticalGoalState.DiffCurrentAndMin)).ToArray();

                //if none are identified, then choose the do-nothing heuristic.
                if (selectedAI.Length == 0)
                    heuristicForActivating = aiForMatchedHeuristics.Select(ai => ai.AssociatedHeuristic).Single(h => h.IsAction == false);
                else
                {
                    selectedAI = selectedAI.GroupBy(ai => ai.Value).OrderBy(hg => hg.Key).First().ToArray();

                    if (selectedAI.Length == 1)
                        heuristicForActivating = selectedAI[0].AssociatedHeuristic;
                    else
                    {
                        heuristicForActivating = selectedAI[LinearUniformRandom.GetInstance.Next(selectedAI.Length)].AssociatedHeuristic;
                    }
                }
            }

        }
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        public void ExecutePartI(Actor actor, LinkedListNode<Period> periodModel, GoalState goalState, IEnumerable<Heuristic> layer, Site site = null)
        {
            Period currentPeriod = periodModel.Value,
                priorPeriod = periodModel.Previous.Value;

            criticalGoalState = goalState;

            Heuristic[] matchedHeuristics = layer.Where(h => h.IsMatch(actor.Variables)).ToArray();

            aiForMatchedHeuristics = actor.AnticipatedInfluences.Where(ai => matchedHeuristics.Contains(ai.AssociatedHeuristic)).ToArray();

            Heuristic activatedHeuristic = null;

            if (aiForMatchedHeuristics.Length > 1)
            {
                priorPeriodActivatedHeuristic = priorPeriod.GetStateForSite(actor, site)
                    .GetActivated(layer.First().Layer);

                SpecificLogic(criticalGoalState.Goal.Tendency);
            }
            else
                activatedHeuristic = aiForMatchedHeuristics[0].AssociatedHeuristic;

            //activatedHeuristic.FreshnessStatus = 0;

            activatedHeuristic.Apply(actor);

            SiteState siteState = currentPeriod.GetStateForSite(actor, site);

            siteState.Activated.Add(activatedHeuristic);
            siteState.Matched.AddRange(matchedHeuristics);
        }
    }
    #endregion
}

