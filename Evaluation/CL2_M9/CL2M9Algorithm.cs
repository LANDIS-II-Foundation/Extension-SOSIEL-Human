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
using Common.Processes;

namespace CL2_M9
{
    public sealed class CL2M9Algorithm : IAlgorithm
    {
        public string Name { get { return "Cognitive level 2 Model 9"; } }

        string _outputFolder;

        AgentList _agentList;

        Configuration<CL2M9Agent> _configuration;

        ProcessConfiguration _processConfig;

        LinkedList<Dictionary<IConfigurableAgent, AgentState>> _iterations = new LinkedList<Dictionary<IConfigurableAgent, AgentState>>();

        List<AgentContributionsOutput> _outputStatistic;


        AnticipatoryLearning al = new AnticipatoryLearning();
        ActionSelection acts = new ActionSelection();
        ActionTaking at = new ActionTaking();

        SocialLearning sl = new SocialLearning();

        public CL2M9Algorithm(Configuration<CL2M9Agent> configuration)
        {
            _configuration = configuration;

            _processConfig = new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                RuleSelectionEnabled = true,
                RuleSelectionPart2Enabled = true
            };

            _outputStatistic = new List<AgentContributionsOutput>(_configuration.AlgorithmConfiguration.IterationCount);

            _outputFolder = @"Output\CL2_M9";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }



        private void Initialize()
        {
            _agentList = AgentList.Generate2(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AgentConfiguration, _configuration.InitialState);
        }

        private void ExecuteAlgorithm()
        {
            throw new NotImplementedException();
        }


        private double CalculateAgentWellbeing(IAgent agent)
        {
            return agent[Agent.VariablesUsedInCode.Engage] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * _agentList.CalculateCommonC() / (double)_agentList.Agents.Count;
        }

        private double CalculateCommonPoolWellbeing(double externalities)
        {
            return externalities * _agentList.CalculateCommonC() / (double)_agentList.Agents.Count;
        }

        public string Run()
        {
            Initialize();


            for (int i = 0; i < _configuration.AlgorithmConfiguration.IterationCount; i++)
            {
                Dictionary<IConfigurableAgent, AgentState> currentIteration;

                if (i > 0)
                    currentIteration = _iterations.AddLast(new Dictionary<IConfigurableAgent, AgentState>()).Value;
                else
                {
                    currentIteration = _iterations.AddLast(IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents.Cast<IConfigurableAgent>())).Value;
                }

                Dictionary<IConfigurableAgent, AgentState> priorIteration = _iterations.Last.Previous?.Value;

                var agentGroups = _agentList.Agents.Cast<IConfigurableAgent>().GroupBy(a => a[Agent.VariablesUsedInCode.AgentType]);

                //rankedGoals is sorted list
                Dictionary<IConfigurableAgent, Goal[]> rankedGoals = new Dictionary<IConfigurableAgent, Goal[]>(_agentList.Agents.Count);

                _agentList.Agents.Cast<IConfigurableAgent>().ForEach(a =>
                {
                    a[Agent.VariablesUsedInCode.Iteration] = i + 1;

                    rankedGoals.Add(a, null);

                    if (i > 0)
                        currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
                });

                if (_processConfig.AnticipatoryLearningEnabled && i > 0)
                {
                    //1st round: AL, CT, IR
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
                        {
                            //Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

                            //currentPeriod.SiteStates.Add(actor, new List<SiteState>(assignedSites.Length));



                            rankedGoals[agent] = al.Execute(agent, _iterations.Last);

                            //if (_processConfig.CounterfactualThinkingEnabled == true)
                            //{
                            //    optimization
                            //    if (rankedGoals[agent].Any(gs => gs.Confidence == false))
                            //    {
                            //        foreach (Site site in assignedSites.Randomize())
                            //        {
                            //            foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            //            {
                            //                //optimization
                            //                GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

                            //                if (criticalGoalState.Confidence == false)
                            //                {
                            //                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                            //                    {
                            //                        //optimization
                            //                        if (layer.Key.LayerParameters.Modifiable)
                            //                        {
                            //                            Heuristic[] matchedPriorPeriodHeuristics = priorPeriod.GetStateForSite(actor, site)
                            //                                    .Matched.Where(h => h.Layer == layer.Key).ToArray();

                            //                            bool? CTResult = null;

                            //                            if (matchedPriorPeriodHeuristics.Length >= 2)
                            //                                counterfactualThinking.Execute(actor, periods.Last, criticalGoalState,
                            //                                matchedPriorPeriodHeuristics, site, layer.Key);


                            //                            if (_processConfig.InnovationEnabled == true)
                            //                            {

                            //                                if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
                            //                                    inductiveReasoning.Execute(actor, periods.Last, criticalGoalState, site, layer.Key);
                            //                            }
                            //                        }
                            //                    }
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }

                if (_processConfig.SocialLearningEnabled && i > 0)
                {
                    //2nd round: SL
                    foreach (var agentGroup in agentGroups)
                    {

                        foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
                        {
                            foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    sl.ExecuteSelection(agent, _iterations.Last.Previous.Value[agent], rankedGoals[agent], layer.Key);
                                }
                            }
                        }

                        sl.ExecuteLearning(agentGroup.ToArray(), _iterations.Last.Previous.Value);
                    }

                }

                if (_processConfig.RuleSelectionEnabled && i > 0)
                {
                    //AS part I
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
                        {
                            foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    acts.ExecutePartI(agent, _iterations.Last, rankedGoals[agent], layer.ToArray());
                                }
                            }
                            
                        }
                    }


                    if (_processConfig.RuleSelectionPart2Enabled)
                    {

                        //4th round: AS part II
                        foreach (var agentGroup in agentGroups)
                        {
                            foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
                            {
                                foreach (var set in agent.AssignedRules.GroupBy(r => r.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        acts.ExecutePartII(agent, _iterations.Last, rankedGoals[agent], layer.ToArray(), _agentList.Agents.Count);
                                    }
                                }
                            }
                        }

                    }
                }

                if (_processConfig.ActionTakingEnabled)
                {
                    //5th round: TA
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IConfigurableAgent agent in agentGroup)
                        {
                            at.Execute(agent, currentIteration[agent]);

                            //if (periods.Last.Value.IsOverconsumption)
                            //    return periods;
                        }
                    }
                }


                Calculations();

                _outputStatistic.Add(new AgentContributionsOutput { Iteration = i + 1, AgentContributions = _agentList.Agents.Select(a => (double)a[Agent.VariablesUsedInCode.AgentC]).ToArray() });

                //Maintenance();
            }


            SaveAgentWellbeingStatistic();


            return _outputFolder;
        }

        void Calculations()
        {
            double poolWellbeing = CalculateCommonPoolWellbeing(_agentList.Agents.First()[Agent.VariablesUsedInCode.MagnitudeOfExternalities]);

            _agentList.Agents.ForEach(a =>
            {
                a[Agent.VariablesUsedInCode.PoolWellbeing] = poolWellbeing;
                a[Agent.VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);
            });
        }


        void SaveAgentWellbeingStatistic()
        {
            ResultSavingHelper.Save(_outputStatistic, $@"{_outputFolder}\contributions_statistic.csv");
        }

    }
}
