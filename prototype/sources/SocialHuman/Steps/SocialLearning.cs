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

        List<Actor> unconfidentActors = new List<Actor>();
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
                unconfidentActors.Add(actor);

        }

        public void ExecuteLearning()
        {
            foreach (Actor actor in unconfidentActors.OrderByDescending(a => a[VariablesName.SocialNetworks].Length))
            {
                string[] socialNetworks = actor[VariablesName.SocialNetworks];

                foreach (string sn in socialNetworks.Reverse())
                {
                    IEnumerable<Actor> suitableActors = confidentActors.Where(kvp => kvp.Key[VariablesName.SocialNetworks].Contains(sn)).Select(kvp => kvp.Key);

                    foreach (Actor sActor in suitableActors)
                    {
                        foreach (Heuristic heuristic in confidentActors[sActor])
                        {
                            if (actor.AssagnedHeuristics.Any(h => h != heuristic)
                                && actor.AssignedGoals.Any(g => heuristic.Layer.Set.AssociatedWith.Any(a => a == g.Goal)))
                            {
                                IEnumerable<AnticipatedInfluence> clonedAI = sActor.AnticipatedInfluences
                                    .Where(ai => ai.AssociatedHeuristic == heuristic && actor.AssignedGoals.Any(gs => gs.Goal == ai.AssociatedGoal))
                                    .Select(ai => (AnticipatedInfluence)ai.Clone());

                                actor.AssagnedHeuristics.Add(heuristic);
                                actor.AnticipatedInfluences.AddRange(clonedAI);
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
