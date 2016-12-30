using System;
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

            //assign household object to actor
            Dictionary<string, Household> households = actors.Where(a => a.Prototype.Type == 2)
                .GroupBy(a => a[VariableNames.Household]).Where(g => g.Key != null)
                .Select(g => new Household(g.Key)).ToDictionary(g => g.Name);

            foreach (Actor actor in actors.Where(a => a.Prototype.Type == 2))
            {
                string householdName = actor[VariableNames.Household];

                actor[VariableNames.Household] = households[householdName];
            }

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
                    IEnumerable<Heuristic> heuristics = actor.AssignedHeuristics;
                    Heuristic[] matchedConditions = heuristics.Where(h => matchedConditionsInPriorPeriod[i].Contains(h.Id)).ToArray();
                    Heuristic[] activatedHeuristics = heuristics.Where(h => activatedHeuristicsInPriorPeriod[i].Contains(h.Id)).ToArray();
                    if (isSiteSpecific)
                    {
                        if (actor[VariableNames.AssignedSites][i])
                        {
                            Site site = sites[i];

                            SiteState ss = SiteState.Create(isSiteSpecific, matchedConditions, activatedHeuristics, site);

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

            algorithm.UpdateDynamicHeuristics();

            algorithm.UpdateHouseholdState();

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
        SocialLearning socialLearning = new SocialLearning();
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

            double totalBiomass = copy.Sum(s => s.BiomassValue);

            actors.Where(a => a.Prototype.Type == 1).ForEach(a => a[VariableNames.TotalBiomass] = totalBiomass);
        }

        void Maintenance()
        {
            UpdateHouseholdState();

            actors.Select(a => a.Prototype).Distinct().SelectMany(p => p.HeuristicEnumerable).ForEach(h => h.FreshnessStatus += 1);

            actors.ForEach(a => a.BlockedHeuristics.Clear());
            actors.ForEach(a => a[VariableNames.Age] += 1);


            UpdateDynamicVariables();

            UpdateDynamicHeuristics();
        }

        void UpdateHouseholdState()
        {
            foreach (var householdMembers in actors.Where(a => a.Prototype.Type == 2).GroupBy(a => a[VariableNames.Household]))
            {
                Household household = householdMembers.Key;

                household.Income += householdMembers.Sum(m => (double)m[VariableNames.Income]);
                household.Expenses += householdMembers.Sum(m => (double)(m[VariableNames.Expenses] ?? 0));

                householdMembers.ForEach(hm => hm[VariableNames.Savings] = household.Savings);
            }
        }

        void UpdateDynamicVariables()
        {

        }

        void UpdateDynamicHeuristics()
        {
            foreach (ActorPrototype prototype in actors.Select(a => a.Prototype).Distinct())
            {
                foreach (Heuristic heuristic in prototype.HeuristicEnumerable)
                {
                    heuristic.Antecedent.Where(ap => ap.Immutable == false).ForEach(ap => ap.Const = prototype[ap.LinkForConst]);

                    if (heuristic.Consequent.LinkToValue != null)
                    {
                        heuristic.Consequent.Value = prototype[heuristic.Consequent.LinkToValue];
                    }
                }
            }
        }

        void SetJobAvailableValue(Period period)
        {
            ActorPrototype[] anotherActorPrototypes = actors.Select(a => a.Prototype).Distinct().Where(p => p.Type != 1).ToArray();

            foreach (Actor type1 in actors.Where(a => a.Prototype.Type == 1))
            {
                bool jobAvailable = false;

                if (period.SiteStates[type1].Any(ss => ss.Activated.Where(g => g.Layer.Set.AssociatedWith.Any(a => a.IsPrimary)).All(h => h.IsAction)))
                {
                    jobAvailable = true;
                }

                type1[VariableNames.JobAvailable] = jobAvailable;

                string paramName = $"{type1.ActorName}_{VariableNames.JobAvailable}";
                anotherActorPrototypes.ForEach(p => p[paramName] = jobAvailable);
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

                var actorGroups = actors.GroupBy(a => a.Prototype).OrderBy(ag => ag.Key.Type).ToList();

                //rankedGoals is sorted list
                Dictionary<Actor, GoalState[]> rankedGoals = new Dictionary<Actor, GoalState[]>(actors.Count);

                //1st round: AL, CT, IR
                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                        currentPeriod.SiteStates.Add(actor, new List<SiteState>(assignedSites.Length));

                        rankedGoals.Add(actor, anticipationLearning.Execute(actor, periods.Last));

                        //optimization
                        if (rankedGoals[actor].Any(gs => gs.Confidence == false))
                        {
                            foreach (Site site in assignedSites.Randomize())
                            {
                                foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    //optimization
                                    GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                                    if (criticalGoalState.Confidence == false)
                                    {
                                        foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                        {
                                            //optimization
                                            if (layer.Key.LayerParameters.Modifiable)
                                            {
                                                Heuristic[] matchedPriorPeriodHeuristics = priorPeriod.GetStateForSite(actor, site)
                                                        .Matched.Where(h => h.Layer == layer.Key).ToArray();

                                                bool? CTResult = null;

                                                if (matchedPriorPeriodHeuristics.Length >= 2)
                                                    counterfactualThinking.Execute(actor, periods.Last, criticalGoalState,
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

                //2nd round: SL
                foreach (var actorGroup in actorGroups)
                {
                    if (actorGroup.Count(a => a[VariableNames.SocialNetworks] != null) >= 2)
                    {
                        foreach (Actor actor in actorGroup.Randomize())
                        {
                            foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                //optimization
                                GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    socialLearning.ExecuteSelection(actor, periods.Last.Previous.Value, criticalGoalState, layer.Key, null);
                                }
                            }
                        }

                        socialLearning.ExecuteLearning(actorGroup.ToArray());
                    }
                }



                //3rd round: HS part I
                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                        List<SiteState> siteStates = new List<SiteState>(assignedSites.Length);

                        foreach (Site site in assignedSites.Randomize())
                        {
                            currentPeriod.SiteStates[actor].Add(SiteState.Create(actor.IsSiteSpecific, site));

                            foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                //optimization
                                GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    heuristicSelection.ExecutePartI(actor, periods.Last, criticalGoalState, layer, site);
                                }
                            }
                        }
                    }

                    if (actorGroup.Key.Type == 1)
                    {
                        SetJobAvailableValue(periods.Last.Value);
                    }
                }



                //4th round: HS part II
                foreach (var actorGroup in actorGroups)
                {
                    foreach (Actor actor in actorGroup.Randomize())
                    {
                        List<Actor> sameTypeActors = actorGroup.ToList();

                        sameTypeActors.Remove(actor);

                        if (sameTypeActors.Count > 0)
                        {
                            Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                            foreach (Site site in assignedSites.Randomize())
                            {
                                foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    //optimization
                                    GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        heuristicSelection.ExecutePartII(actor, sameTypeActors, periods.Last, criticalGoalState, layer, site);
                                    }
                                }
                            }
                        }
                    }
                }

                //5th round: TA
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
