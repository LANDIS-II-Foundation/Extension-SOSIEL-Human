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
        #endregion

        #region Public fields



        #endregion

        #region Constructors
        #endregion

        #region Private methods
        
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
        }

        public void ExecuteLearning(Actor[] allActors)
        {
            foreach (var actor in allActors)
            {
                string[] socialNetworks = actor[VariableNames.SocialNetworks];

                if (socialNetworks.Length > 0)
                {
                    foreach (string sn in socialNetworks)
                    {
                        IEnumerable<Actor> suitableActors = confidentActors.Keys.Where(a => ((string[])a[VariableNames.SocialNetworks]).Contains(sn) && a != actor);

                        foreach (Actor sActor in suitableActors)
                        {
                            foreach (Heuristic heuristic in confidentActors[sActor].Where(h => h.Layer.Set.AssociatedWith.Any(g=> actor.AssignedGoals.Any(ag=> g.Equals(ag.Goal)))))
                            {
                                if (actor.AssignedHeuristics.Any(h => h != heuristic))
                                {
                                    //if (heuristic.IsCollectiveAction)
                                    //{
                                    //    List<Actor> relatedActors = allActors.Where(a => ((string[])a[VariableNames.SocialNetworks])
                                    //        .Any(s => socialNetworks.Contains(s))).ToList();

                                    //    relatedActors.Remove(sActor);

                                    //    relatedActors.ForEach(a => AssignHeuristic(a, sActor, heuristic));
                                    //}
                                    //else
                                    //{
                                        actor.AssignHeuristic(sActor, heuristic);
                                    //}
                                }
                            }
                        }
                    }
                }
            }

            //clean state
            confidentActors.Clear();
        }
        #endregion
    }
}
