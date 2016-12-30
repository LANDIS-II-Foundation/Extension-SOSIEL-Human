using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Steps
{
    using Enums;
    using Models;
    using Helpers;


    sealed class SocialLearning
    {
        #region Private fields

        Dictionary<Actor, List<Heuristic>> confidentActors = new Dictionary<Actor, List<Heuristic>>();

        Dictionary<Actor, Goal> unconfidentActors = new Dictionary<Actor, Goal>();
        #endregion

        #region Public fields



        #endregion

        #region Constructors
        #endregion

        #region Private methods
        private void AssignHeuristic(Actor actor, Actor suitableActor, Heuristic heuristic)
        {
            IEnumerable<AnticipatedInfluence> clonedAI = suitableActor.AnticipatedInfluences
                .Where(ai => ai.AssociatedHeuristic == heuristic && actor.AssignedGoals.Any(gs => gs.Goal.Equals(ai.AssociatedGoal)))
                .Select(ai => (AnticipatedInfluence)ai.Clone());

            actor.AssignedHeuristics.Add(heuristic);
            actor.AnticipatedInfluences.AddRange(clonedAI);
        }
        #endregion

        #region Public methods
        public void ExecuteSelection(Actor actor, Period priorPeriod, GoalState goalState, HeuristicLayer layer, Site site)
        {
            if (goalState.Confidence)
            {
                Heuristic heuristic = priorPeriod.GetStateForSite(actor, site).Activated.Single(h => h.Layer == layer);

                if (confidentActors.ContainsKey(actor))
                    confidentActors[actor].Add(heuristic);
                else
                    confidentActors.Add(actor, new List<Heuristic>() { heuristic });
            }
            else
                if(unconfidentActors.ContainsKey(actor) == false)
                    unconfidentActors.Add(actor, goalState.Goal);

        }

        public void ExecuteLearning(Actor[] allActors)
        {
            foreach (var actor in unconfidentActors.OrderByDescending(a => a.Key[VariableNames.SocialNetworks].Length))
            {
                string[] socialNetworks = actor.Key[VariableNames.SocialNetworks];

                foreach (string sn in socialNetworks.Reverse())
                {
                    IEnumerable<Actor> suitableActors = confidentActors.Where(kvp => ((string[])kvp.Key[VariableNames.SocialNetworks]).Contains(sn))
                        .Where(kvp => kvp.Value.Any(h => h.Layer.Set.AssociatedWith.Contains(actor.Value)))
                        .Select(kvp => kvp.Key);

                    foreach (Actor sActor in suitableActors)
                    {
                        foreach (Heuristic heuristic in confidentActors[sActor].Where(h => h.Layer.Set.AssociatedWith.Contains(actor.Value)))
                        {
                            if (actor.Key.AssignedHeuristics.Any(h => h != heuristic)
                                                && actor.Key.AssignedGoals.Any(g => heuristic.Layer.Set.AssociatedWith.Any(a => a.Equals(g.Goal)))
                                                && heuristic.IsMatch((param) => actor.Key[param]))
                            {

                                if (heuristic.IsCollectiveAction)
                                {
                                    List<Actor> relatedActors = allActors.Where(a => ((string[])a[VariableNames.SocialNetworks])
                                        .Any(s => socialNetworks.Contains(s))).ToList();

                                    relatedActors.Remove(sActor);

                                    relatedActors.ForEach(a => AssignHeuristic(a, sActor, heuristic));
                                }
                                else
                                {
                                    AssignHeuristic(actor.Key, sActor, heuristic);
                                }
                            }
                        }
                    }
                }
            }

            //clean state
            confidentActors.Clear();
            unconfidentActors.Clear();
        }
        #endregion
    }
}
