using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using Common.Configuration;
using Common.Algorithm;
using Common.Entities;
using Common.Helpers;
using Common.Randoms;
using Common.Models;

namespace CL2_M7
{
    public sealed class CL2M7Algorithm : IAlgorithm
    {
        public string Name { get { return "Cognitive level 2 Model 7"; } }

        string _outputFolder;

        AgentList _agentList;

        Configuration<CL2M7Agent> _configuration;

        ProcessConfig _processConfig;

        LinkedList<Dictionary<IConfigurableAgent, AgentState>> _iterations = new LinkedList<Dictionary<IConfigurableAgent, AgentState>>();

        public CL2M7Algorithm(Configuration<CL2M7Agent> configuration)
        {
            _configuration = configuration;

            _processConfig = new ProcessConfig { ActionTakingEnabled = true, AnticipatoryLearningEnabled = true, RuleSelectionEnabled = true };


            _outputFolder = @"Output\CL2_M7";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }



        private void Initialize()
        {
            _agentList = AgentList.Generate2(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AgentConfiguration, _configuration.InitialState);

            _iterations.AddLast(IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents.Cast<IConfigurableAgent>()));

        }

        private void ExecuteAlgorithm()
        {
            throw new NotImplementedException();
        }


        private double CalculateAgentWellbeing(IAgent agent, Site centerSite)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Run()
        {
            Initialize();

            for (int i = 0; i < _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                Dictionary<IConfigurableAgent, AgentState> currentPeriod = _iterations.AddLast(new Dictionary<IConfigurableAgent, AgentState>()).Value;
                Dictionary<IConfigurableAgent, AgentState> priorPeriod = _iterations.Last.Previous.Value;

                var agentGroups = _agentList.Agents.GroupBy(a=>a[Agent.VariablesUsedInCode.AgentType]);

                //rankedGoals is sorted list
                Dictionary<IAgent, GoalState[]> rankedGoals = new Dictionary<IAgent, GoalState[]>(_agentList.Agents.Count);

                //1st round: AL, CT, IR
                foreach (var agentGroup in agentGroups)
                {
                    foreach (IAgent agent in agentGroup/*.Randomize()*/)
                    {
                        //Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                        //currentPeriod.SiteStates.Add(actor, new List<SiteState>(assignedSites.Length));

                        rankedGoals.Add(actor, anticipationLearning.Execute(actor, periods.Last));

                        //optimization
                        if (rankedGoals[agent].Any(gs => gs.Confidence == false))
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

                ////2nd round: SL
                //foreach (var actorGroup in actorGroups)
                //{
                //    if (actorGroup.Count(a => a[VariableNames.SocialNetworks] != null) >= 2)
                //    {
                //        foreach (Actor actor in actorGroup.Randomize())
                //        {
                //            foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                //            {
                //                //optimization
                //                GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                //                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                //                {
                //                    socialLearning.ExecuteSelection(actor, periods.Last.Previous.Value, criticalGoalState, layer.Key, null);
                //                }
                //            }
                //        }

                //        socialLearning.ExecuteLearning(actorGroup.ToArray());
                //    }
                //}



                ////3rd round: HS part I
                //foreach (var actorGroup in actorGroups)
                //{
                //    foreach (Actor actor in actorGroup.Randomize())
                //    {
                //        Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                //        List<SiteState> siteStates = new List<SiteState>(assignedSites.Length);

                //        foreach (Site site in assignedSites.Randomize())
                //        {
                //            currentPeriod.SiteStates[actor].Add(SiteState.Create(actor.IsSiteSpecific, site));

                //            foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                //            {
                //                //optimization
                //                GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                //                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                //                {
                //                    heuristicSelection.ExecutePartI(actor, actorGroup.ToArray(), periods.Last, criticalGoalState, layer, site);
                //                }
                //            }
                //        }
                //    }

                //    if (actorGroup.Key.Type == 1)
                //    {
                //        SetJobAvailableValue(periods.Last.Value);
                //    }
                //}



                ////4th round: HS part II
                //foreach (var actorGroup in actorGroups)
                //{
                //    foreach (Actor actor in actorGroup.Randomize())
                //    {
                //        List<Actor> sameTypeActors = actorGroup.ToList();

                //        sameTypeActors.Remove(actor);

                //        if (sameTypeActors.Count > 0)
                //        {
                //            Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                //            foreach (Site site in assignedSites.Randomize())
                //            {
                //                foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                //                {
                //                    //optimization
                //                    GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                //                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                //                    {
                //                        heuristicSelection.ExecutePartII(actor, sameTypeActors, periods.Last, criticalGoalState, layer, site);
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                ////5th round: TA
                //foreach (var actorGroup in actorGroups)
                //{
                //    foreach (Actor actor in actorGroup)
                //    {
                //        takeAction.Execute(actor, periods.Last, sites);

                //        if (periods.Last.Value.IsOverconsumption)
                //            return periods;
                //    }
                //}

                //Maintenance();
            }




            return _outputFolder;
        }
    }
}
