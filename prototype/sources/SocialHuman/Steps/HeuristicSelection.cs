using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Enums;
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
            AnticipatedInfluence[] selectedAI = new AnticipatedInfluence[] { };

            if (criticalGoalState.DiffCurrentAndMin > 0)
            {
                if (aiForMatchedHeuristics.Any(ai => ai.AssociatedHeuristic == priorPeriodActivatedHeuristic))
                {
                    heuristicForActivating = priorPeriodActivatedHeuristic;
                    return;
                }
                else
                {
                    selectedAI = aiForMatchedHeuristics.Where(ai => ai.Value >= 0 &&
                        ai.Value < criticalGoalState.DiffCurrentAndMin).ToArray();
                }
            }
            else
            {
                Goal goal = criticalGoalState.Goal;

                selectedAI = aiForMatchedHeuristics.Where(ai => ai.Value >= 0 &&
                    ai.Value > criticalGoalState.DiffCurrentAndMin).ToArray();
            }

            if (selectedAI.Length > 0)
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

        protected override void BelowMax()
        {
            AnticipatedInfluence[] selectedAI = new AnticipatedInfluence[] { };

            if (criticalGoalState.DiffCurrentAndMin < 0)
            {
                if (aiForMatchedHeuristics.Any(ai => ai.AssociatedHeuristic == priorPeriodActivatedHeuristic))
                {
                    heuristicForActivating = priorPeriodActivatedHeuristic;
                    return;
                }
                else
                {
                    selectedAI = aiForMatchedHeuristics.Where(ai => ai.Value <= 0 &&
                        Math.Abs(ai.Value) < Math.Abs(criticalGoalState.DiffCurrentAndMin)).ToArray();
                }
            }
            else
            {
                Goal goal = criticalGoalState.Goal;

                selectedAI = aiForMatchedHeuristics.Where(ai => ai.Value < 0 &&
                    Math.Abs(ai.Value) > Math.Abs(criticalGoalState.DiffCurrentAndMin)).ToArray();
            }

            if (selectedAI.Length > 0)
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
        #endregion

        #region Private methods
        void ShareCollectiveAction(ICollection<Actor> sameTypeActors, Actor currentActor, Heuristic heuristic)
        {
            string socialNetwork = ((string[])currentActor[VariableNames.SocialNetworks])[0];

            foreach (Actor actor in sameTypeActors.Where(a => ((string[])a[VariableNames.SocialNetworks]).Any(sn => sn == socialNetwork) && a != currentActor))
            {
                actor.AssignHeuristic(currentActor, heuristic);
            }
        }
        #endregion

        #region Public methods
        public void ExecutePartI(Actor actor, ICollection<Actor> sameTypeActors, LinkedListNode<Period> periodModel, GoalState goalState, IEnumerable<Heuristic> layer, Site site = null)
        {
            heuristicForActivating = null;

            Period currentPeriod = periodModel.Value,
                priorPeriod = periodModel.Previous.Value;

            criticalGoalState = goalState;

            Heuristic[] matchedHeuristics = layer.Except(actor.BlockedHeuristics).Where(h => h.IsMatch((param) => actor[param])).ToArray();

            aiForMatchedHeuristics = actor.AnticipatedInfluences.Where(ai => matchedHeuristics.Contains(ai.AssociatedHeuristic) && ai.AssociatedGoal.Equals(goalState.Goal)).ToArray();

            if (aiForMatchedHeuristics.Length > 1)
            {
                priorPeriodActivatedHeuristic = priorPeriod.GetStateForSite(actor, site)
                    .GetActivated(layer.First().Layer);

                SpecificLogic(criticalGoalState.Goal.Tendency);

                //if none are identified, then choose the do-nothing heuristic.
                if (heuristicForActivating == null)
                    heuristicForActivating = aiForMatchedHeuristics.Select(ai => ai.AssociatedHeuristic).Single(h => h.IsAction == false);
            }
            else
                heuristicForActivating = aiForMatchedHeuristics[0].AssociatedHeuristic;

            //activatedHeuristic.FreshnessStatus = 0;



            heuristicForActivating.Apply(actor);

            if (heuristicForActivating.IsCollectiveAction)
            {
                ShareCollectiveAction(sameTypeActors, actor, heuristicForActivating);
            }


            SiteState siteState = currentPeriod.GetStateForSite(actor, site);

            siteState.Activated.Add(heuristicForActivating);
            siteState.Matched.AddRange(matchedHeuristics);
        }

        public void ExecutePartII(Actor actor, ICollection<Actor> sameTypeActors, LinkedListNode<Period> periodModel, GoalState goalState, IEnumerable<Heuristic> layer, Site site = null)
        {
            Period currentPeriod = periodModel.Value;

            HeuristicLayer heuristicLayer = layer.First().Layer;

            Heuristic selectedHeuristic = currentPeriod.GetStateForSite(actor, site).Activated.Single(h => h.Layer == heuristicLayer);

            if (selectedHeuristic.IsCollectiveAction)
            {
                int actorsCount = sameTypeActors.Count(a => currentPeriod.GetStateForSite(a, site).Activated.Single(h => h.Layer == heuristicLayer) == selectedHeuristic);

                if (actorsCount + 1 < selectedHeuristic.RequiredParticipants)
                {
                    actor.BlockedHeuristics.Add(selectedHeuristic);
                    SiteState siteState = currentPeriod.GetStateForSite(actor, site);

                    siteState.Activated.Clear();
                    siteState.Matched.Clear();

                    ExecutePartI(actor, sameTypeActors, periodModel, goalState, layer, site);
                }
            }
        }
    }
    #endregion
}

