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
        public static Algorithm Initialize(GlobalParameters globalParameters, ActorParameters[] actors)
        {
            Global.Init(globalParameters);

            Algorithm algorithm = new Algorithm
            {
                actors = new List<Actor>(actors.Length),
                periods = new LinkedList<PeriodModel>(),
                biomassGrowthRate = globalParameters.BiomassGrowthRate,
                periodsCount = globalParameters.PeriodsCount,
                sites = globalParameters.BiomassBySite.Select(s => new Site { GoalValue = s }).ToArray()
            };

            Site[] sites = algorithm.sites;

            PeriodModel zeroPeriodModel = new PeriodModel(0, sites);
            List<SiteModel> partialData = zeroPeriodModel.PartialData;

            foreach (ActorParameters actorParameters in actors)
            {
                Actor newActor = ActorFactory.Create(actorParameters, sites);
                algorithm.actors.Add(newActor);


                string[][] matchedConditionsInPriorPeriod = actorParameters.MatchedConditionsInPriorPeriod;
                string[][] activatedHeuristicsInPriorPeriod = actorParameters.ActivatedHeuristicsInPriorPeriod;

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

                        SiteModel partialModel = SiteModel.Create(newActor, site,
                            matchedConditionsForSiteInPriorPeriod, activatedHeuristicsForSiteInPriorPeriod);

                        partialData.Add(partialModel);
                    }
                }


            }

            algorithm.periods.AddFirst(zeroPeriodModel);



            return algorithm;
        }

        List<Actor> actors;
        LinkedList<PeriodModel> periods;
        double[] biomassGrowthRate;

        int periodsCount;
        Site[] sites;

        private Algorithm() { }

        void BiomassGrowth(double growthRate)
        {
            //create copy of sites state
            Site[] copy = sites.Select(s => (Site)s.Clone()).ToArray();

            for (int i = 0; i < sites.Length; i++)
            {
                copy[i].GoalValue *= growthRate;
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

        public LinkedList<PeriodModel> Run()
        {
            //int[,] bla3 = new int[4, 10];
            //int[][] bla = new int[10000][];
            //for (int i = 0; i < 10000; i++)
            //{
            //    bla[i] = new int[4];
            //}
            //string[] bla2 = new string[10000];
            //for (int t = 1; t < 5; t++)
            //{


            //    for (int i = 0; i < 10000; i++)
            //    {
            //        bla[i][t-1] = (int)Randoms.PowerLawRandom.GetInstance.Next(0, 100);

            //        var col = (bla[i][t - 1] - 1) / 10;

            //        bla3[t - 1, col] += 1;


            //        if(t==4)
            //        {
            //            bla2[i] = string.Join(";", bla[i]);
            //        }

            //    }
            //}

            //var l = string.Join(Environment.NewLine, bla2);

            //System.IO.File.WriteAllText("output.csv", l);

            for (int period = 0; period < periodsCount; period++)
            {
                int actualPaeriodNumber = period + 1;

                Maintenance();

                BiomassGrowth(biomassGrowthRate[period]);

                var currentPeriod = periods.AddLast(new PeriodModel(actualPaeriodNumber, sites));

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
            }

            return periods;
        }
    }
}
