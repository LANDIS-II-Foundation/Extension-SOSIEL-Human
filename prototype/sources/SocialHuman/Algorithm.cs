using System.Collections.Generic;
using System.Linq;

namespace SocialHuman
{
    using Actors;
    using Enums;
    using Entities;
    using Models;
    using Helpers;

    public class Algorithm
    {
        #region Factory methods
        public static Algorithm Initialize(GlobalParameters globalParameters, ActorParameters[] actors,
            Dictionary<string, PeriodInitialStateParameters> initialState)
        {
            Global.Init(globalParameters);

            Algorithm algorithm = new Algorithm
            {
                actors = new List<Actor>(actors.Length),
                periods = new LinkedList<Period>(),
                biomassGrowthRate = globalParameters.BiomassGrowthRate,
                periodsCount = globalParameters.PeriodsCount,
                sites = globalParameters.BiomassBySite.Select(s => new Site { BiomassValue = s }).ToArray()
            };

            Site[] sites = algorithm.sites;

            Period zeroPeriodModel = new Period(0, sites);


            foreach (ActorParameters actorParameters in actors)
            {
                Actor newActor = ActorFactory.Create(actorParameters, sites);
                algorithm.actors.Add(newActor);

                PeriodInitialStateParameters periodStateForActor = initialState[actorParameters.ActorName];

                #region Create site states
                List<SiteState> siteStates = new List<SiteState>(sites.Length);
                List<TakeActionState> takeActions = new List<TakeActionState>(sites.Length);

                string[][] matchedConditionsInPriorPeriod = periodStateForActor.MatchedConditionsInPriorPeriod;
                string[][] activatedHeuristicsInPriorPeriod = periodStateForActor.ActivatedHeuristicsInPriorPeriod;

                for (int i = 0; i < sites.Length; i++)
                {
                    if (actorParameters.AssignedSites[i])
                    {
                        Site site = sites[i];

                        Heuristic[] allHeuristics = newActor.MentalModel.SelectMany(s => s.Layers.SelectMany(l => l.Heuristics)).ToArray();

                        Heuristic[] matchedConditionsForSiteInPriorPeriod =
                            allHeuristics.Where(h => matchedConditionsInPriorPeriod[i].Contains(h.Id)).ToArray();
                        Heuristic[] activatedHeuristicsForSiteInPriorPeriod =
                            allHeuristics.Where(h => activatedHeuristicsInPriorPeriod[i].Contains(h.Id)).ToArray();

                        SiteState newSiteData = SiteState.Create(site,
                            matchedConditionsForSiteInPriorPeriod, activatedHeuristicsForSiteInPriorPeriod);

                        //todo: only for one heuristic set now
                        newSiteData.TakeActions.Add(new TakeActionState(newActor.MentalModel.First(), periodStateForActor.Harvested[i]));

                        siteStates.Add(newSiteData);
                    }
                }

                zeroPeriodModel.SiteStates.Add(newActor, siteStates);
                //zeroPeriodModel.TakeActions.Add(newActor, takeActions);
                #endregion

                #region Create goals state for actor  
                List<ActorGoalState> goalsState = new List<ActorGoalState>(newActor.Goals.Length);

                foreach (ActorGoal goal in newActor.Goals)
                {
                    GoalStateParameters goalState = periodStateForActor.GoalsState.Single(gp => gp.GoalName == goal.Name);

                    goalsState.Add(new ActorGoalState(goal, goalState.GoalValue));
                }

                zeroPeriodModel.GoalStates.Add(newActor, goalsState);
                #endregion
            }

            algorithm.periods.AddFirst(zeroPeriodModel);


            return algorithm;
        }
        #endregion

        #region Private fields
        List<Actor> actors;
        LinkedList<Period> periods;
        double[] biomassGrowthRate;

        int periodsCount;
        Site[] sites;
        #endregion

        #region Constructors
        private Algorithm() { }
        #endregion

        #region Private methods
        void BiomassGrowth(double growthRate)
        {
            //create copy of sites state
            Site[] copy = sites.Select(s => (Site)s.Clone()).ToArray();

            for (int i = 0; i < sites.Length; i++)
            {
                copy[i].BiomassValue *= growthRate;
            }

            //save new state
            sites = copy;
        }

        void Maintenance()
        {
            foreach (Actor actor in actors)
            {
                IEnumerable<Heuristic> heuristics = actor.MentalModel.SelectMany(s => s.Layers).SelectMany(l => l.Heuristics);

                foreach (Heuristic heuristic in heuristics)
                {
                    heuristic.FreshnessStatus += 1;
                }
            }
        }
        #endregion

        #region Public methods
        public LinkedList<Period> Run()
        {
            for (int period = 0; period < periodsCount; period++)
            {
                int actualPeriodNumber = period + 1;

                BiomassGrowth(biomassGrowthRate[period]);

                var currentPeriod = periods.AddLast(new Period(actualPeriodNumber, sites));

                var actorGroups = actors.GroupBy(a => a.ActorType).OrderBy(ag => ag.Key);

                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        actor.SimulatePart1(currentPeriod);
                    }
                }

                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        actor.SimulatePart2(currentPeriod);
                    }
                }

                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup)
                    {
                        actor.SimulateTakeActionPart(currentPeriod);

                        if (currentPeriod.Value.IsOverconsumption)
                            return periods;
                    }
                }

                Maintenance();
            }

            return periods;
        }
        #endregion
    }

}
