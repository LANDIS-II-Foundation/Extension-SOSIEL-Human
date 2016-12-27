using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace SocialHuman
{
    using Enums;
    using Helpers;
    using Models;
    using Parsers;
    using Steps;
    using Input = Parsers.Models;

    public class Algorithm
    {
        #region Static methods
        public static void ConfigureMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Input.ActorPrototype, ActorPrototype>();
                cfg.CreateMap<Input.Goal, Goal>();
                cfg.CreateMap<Input.Heuristic, Heuristic>().ForMember(m => m.Layer, opt => opt.Ignore());
                cfg.CreateMap<Input.HeuristicLayerParameters, HeuristicLayerParameters>();
                cfg.CreateMap<Input.HeuristicAntecedentPart, HeuristicAntecedentPart>();
                cfg.CreateMap<Input.HeuristicConsequentPart, HeuristicConsequentPart>();
            });
        }
        #endregion

        #region Factory methods
        public static Algorithm Initialize(string inputJsonPath)
        {
            ConfigureMapper();

            IParser parser = new JsonParser(inputJsonPath);

            Input.GlobalInput configuration = parser.ParseGlogalConfiguration();
            Dictionary<string, Input.ActorPrototype> actorPrototypes = parser.ParseActorPrototypes();
            Dictionary<string, Input.InitialState> initialStates = parser.ParseInitialState();

            Global.Init(configuration);

            Site[] sites = configuration.BiomassBySite.Select(s => new Site { BiomassValue = s }).ToArray();

            Dictionary<string, ActorPrototype> availablePrototypes = actorPrototypes.ToDictionary(kvp => kvp.Key, kvp => ActorPrototype.Create(kvp.Value));

            List<Actor> actors = initialStates.Select(kvp => Actor.Create(kvp.Key, availablePrototypes[kvp.Value.Class], kvp.Value)).ToList();

            Period zeroPeriodModel = new Period(0, sites);


            foreach (var initialState in initialStates)
            {
                Actor actor = actors.Single(a => a.ActorName == initialState.Key);


                #region Create site states
                List<SiteState> siteStates = new List<SiteState>(sites.Length);
                //List<TakeActionState> takeActions = new List<TakeActionState>(sites.Length);

                string[][] matchedConditionsInPriorPeriod = initialState.Value.MatchedConditionsInPriorPeriod;
                string[][] activatedHeuristicsInPriorPeriod = initialState.Value.ActivatedHeuristicsInPriorPeriod;

                bool isSiteSpecific = actor.IsSiteSpecific;

                for (int i = 0; i < sites.Length; i++)
                {
                    IEnumerable<Heuristic> heuristics = actor.AssagnedHeuristics;
                    Heuristic[] matchedConditions = heuristics.Where(h => matchedConditionsInPriorPeriod[i].Contains(h.Id)).ToArray();
                    Heuristic[] activatedHeuristics = heuristics.Where(h => activatedHeuristicsInPriorPeriod[i].Contains(h.Id)).ToArray();
                    if (isSiteSpecific)
                    {
                        if (actor[VariablesName.AssignedSites][i])
                        {
                            Site site = sites[i];

                            SiteState ss = SiteState.Create(isSiteSpecific, matchedConditions, activatedHeuristics, site);

                            //todo think about it
                            //ss.TakeActions.Add(new TakeActionState(VariablesName.Harvested, actor[VariablesName.Harvested][i]));

                            siteStates.Add(ss);
                        }
                    }
                    else
                    {
                        siteStates.Add(SiteState.Create(isSiteSpecific, matchedConditions, activatedHeuristics));
                        break;
                    }
                }

                zeroPeriodModel.SiteStates.Add(actor, siteStates);
                #endregion

                //#region Create goals state for actor  
                //List<GoalState> goalsState = new List<GoalState>(actor.AssignedGoals.Length);

                //foreach (GoalState goalState in actor.AssignedGoals)
                //{
                //    GoalStateParameters goalState = periodStateForActor.GoalsState.Single(gp => gp.GoalName == goal.Name);

                //    goalsState.Add(new GoalState(goal, goalState.GoalValue));
                //}

                //zeroPeriodModel.GoalStates.Add(newActor, goalsState);
                //#endregion
            }

            Algorithm algorithm = new Algorithm
            {
                actors = actors,
                periods = new LinkedList<Period>(),
                biomassGrowthRate = configuration.BiomassGrowthRate,
                periodsCount = configuration.PeriodsCount,
                sites = sites
            };

            algorithm.periods.AddFirst(zeroPeriodModel);

            return algorithm;
        }
        #endregion

        #region Public fields
        public IEnumerable<Actor> Actors { get { return actors; } }
        #endregion

        #region Private fields
        List<Actor> actors;
        LinkedList<Period> periods;
        double[] biomassGrowthRate;

        int periodsCount;
        Site[] sites;

        AnticipationLearning anticipationLearning = new AnticipationLearning();
        CounterfactualThinking counterfactualThinking = new CounterfactualThinking();
        InductiveReasoning inductiveReasoning = new InductiveReasoning();
        HeuristicSelection heuristicSelection = new HeuristicSelection();
        TakeAction takeAction = new TakeAction();
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
            foreach (ActorPrototype actorPrototype in actors.Select(a => a.Prototype).Distinct())
            {
                foreach (Heuristic heuristic in actorPrototype.HeuristicEnumerable)
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

                Period currentPeriod = periods.AddLast(new Period(actualPeriodNumber, sites)).Value;
                Period priorPeriod = periods.Last.Previous.Value;

                var actorGroups = actors.GroupBy(a => a.Prototype).OrderBy(ag => ag.Key.Type);

                GoalState[] rankedGoals = null;

                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                        currentPeriod.SiteStates.Add(actor, new List<SiteState>(assignedSites.Length));

                        rankedGoals = anticipationLearning.Execute(actor, periods.Last);

                        //optimization
                        if (rankedGoals.Any(gs => gs.Confidence == false))
                        {
                            foreach (Site site in assignedSites.Randomize())
                            {
                                foreach (var set in actor.AssagnedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    //optimization
                                    //goals is sorted list
                                    GoalState criticalGoalState = rankedGoals.First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        //optimization
                                        if (layer.Key.LayerParameters.Modifiable)
                                        {
                                            Heuristic[] matchedPriorPeriodHeuristics = priorPeriod.GetStateForSite(actor, site)
                                                    .Matched.Where(h => h.Layer == layer.Key).ToArray();

                                            if (criticalGoalState.Confidence == false)
                                            {
                                                bool? CTResult = counterfactualThinking.Execute(actor, periods.Last, criticalGoalState,
                                                    matchedPriorPeriodHeuristics, site, layer.Key);

                                                if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
                                                    inductiveReasoning.Execute(actor, periods.Last, criticalGoalState, site, layer.Key);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                        currentPeriod.SiteStates.Add(actor, new List<SiteState>(assignedSites.Length));

                        foreach (Site site in assignedSites.Randomize())
                        {
                            currentPeriod.SiteStates[actor].Add(SiteState.Create(actor.IsSiteSpecific, site));

                            foreach (var set in actor.AssagnedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                //optimization
                                //goals is sorted list
                                GoalState criticalGoalState = rankedGoals.First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    heuristicSelection.ExecutePartI(actor, periods.Last, criticalGoalState, layer, site);
                                }
                            }
                        }
                    }
                }

                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup)
                    {
                        takeAction.Execute(actor, periods.Last, sites);

                        if (periods.Last.Value.IsOverconsumption)
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
