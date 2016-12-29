using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Steps
{
    using Enums;
    using Models;


    sealed class SocialLearning
    {
        #region Private fields

        Dictionary<Actor, List<Heuristic>> confidentActors = new Dictionary<Actor, List<Heuristic>>();

        Dictionary<Actor, Goal> unconfidentActors = new Dictionary<Actor, Goal>();
        #endregion

        #region Public fields
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
                unconfidentActors.Add(actor, goalState.Goal);

        }

        public void ExecuteLearning()
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
                                && actor.Key.AssignedGoals.Any(g => heuristic.Layer.Set.AssociatedWith.Any(a => a.Equals(g.Goal))))
                            {
                                IEnumerable<AnticipatedInfluence> clonedAI = sActor.AnticipatedInfluences
                                    .Where(ai => ai.AssociatedHeuristic == heuristic && actor.Key.AssignedGoals.Any(gs => gs.Goal.Equals(ai.AssociatedGoal)))
                                    .Select(ai => (AnticipatedInfluence)ai.Clone());

                                actor.Key.AssignedHeuristics.Add(heuristic);
                                actor.Key.AnticipatedInfluences.AddRange(clonedAI);
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

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        #endregion
    }
}
