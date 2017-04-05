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

namespace CL3_M10
{
    public sealed class CL3M10Algorithm : SosielAlgorithm<CL3M10Agent>, IAlgorithm
    {
        public string Name { get { return "Cognitive level 3 Model 10"; } }

        string _outputFolder;

        Configuration<CL3M10Agent> _configuration;


        //statistics
        List<SubtypeProportionOutput> _subtypeProportionStatistic;
        List<CommonPoolSubtypeFrequencyOutput> _commonPoolFrequencyStatistic;
        List<ValuesOutput> _valuesOutput;


        public static ProcessConfiguration GetProcessConfiguration()
        {
            return new ProcessConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                RuleSelectionEnabled = true,
                RuleSelectionPart2Enabled = true,
                SocialLearningEnabled = true,
                AgentRandomizationEnabled = true,
            };
        }

        public CL3M10Algorithm(Configuration<CL3M10Agent> configuration) : base(configuration.AlgorithmConfiguration, GetProcessConfiguration())
        {
            _configuration = configuration;

            //statistics
            _subtypeProportionStatistic = new List<SubtypeProportionOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _commonPoolFrequencyStatistic = new List<CommonPoolSubtypeFrequencyOutput>(_configuration.AlgorithmConfiguration.IterationCount);
            _valuesOutput = new List<ValuesOutput>(_configuration.AlgorithmConfiguration.IterationCount);


            _outputFolder = @"Output\CL4_M11";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }

        public string Run()
        {
            ExecuteAlgorithm();

            return _outputFolder;
        }

        protected override void InitializeAgents()
        {
            _agentList = AgentList.Generate2(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AgentConfiguration, _configuration.InitialState);
        }

        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            return IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents);
        }

        protected override void AfterInitialization()
        {
            //StatisticHelper.SaveState(_outputFolder, "initial", _agentList.ActiveAgents, _siteList);


            //_subtypeProportionStatistic.Add(StatisticHelper.CreateSubtypeProportionRecord(_agentList.ActiveAgents, 0, (int)AgentSubtype.Co));
        }

        protected override void AfterAlgorithmExecuted()
        {
            //StatisticHelper.SaveState(_outputFolder, "final", _agentList.ActiveAgents, _siteList);



        }

        protected override void PreIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PreIterationCalculations(iteration, orderedAgents);

            _agentList.Agents.First().SetToCommon(Agent.VariablesUsedInCode.Iteration, iteration);

            //LookingForBetterSites(orderedAgents);
        }

        protected override void PostIterationCalculations(int iteration, IAgent[] orderedAgents)
        {
            base.PostIterationCalculations(iteration, orderedAgents);

            IAgent agent = _agentList.Agents.First();

            UpdateEndowment();

            orderedAgents.AsParallel().ForAll(a =>
            {
                Site currentSite = a[Agent.VariablesUsedInCode.AgentCurrentSite];

                //a[Agent.VariablesUsedInCode.AgentSubtype] = a[Agent.VariablesUsedInCode.AgentC] > 0 ? AgentSubtype.Co : AgentSubtype.NonCo;


                a[Agent.VariablesUsedInCode.NeighborhoodSize] = currentSite.GroupSize;
                a[Agent.VariablesUsedInCode.NeighborhoodVacantSites] = currentSite.GroupSize - a.ConnectedAgents.Count;


                a[Agent.VariablesUsedInCode.CommonPoolUnalike] = a.ConnectedAgents.Count(a2 => a2[Agent.VariablesUsedInCode.AgentSubtype] != a[Agent.VariablesUsedInCode.AgentSubtype]);
                a[Agent.VariablesUsedInCode.CommonPoolSize] = a[Agent.VariablesUsedInCode.NeighborhoodSize] + 1 - a[Agent.VariablesUsedInCode.NeighborhoodVacantSites];

                a[Agent.VariablesUsedInCode.CommonPoolC] = a.ConnectedAgents.Sum(a2 => a2[Agent.VariablesUsedInCode.AgentC]) + a[Agent.VariablesUsedInCode.AgentC];



                a[Agent.VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);

                a[Agent.VariablesUsedInCode.PoolWellbeing] = CalculatePoolWellbeing(a);

            });



        }




        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            IAgent[] activeAgents = _agentList.ActiveAgents;

           
        }


        private double CalculateAgentWellbeing(IAgent agent)
        {
            int commonPoolC = agent.ConnectedAgents.Sum(a => a[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.AgentE] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)agent.ConnectedAgents.Count + 1);
        }

        private double CalculateAgentWellbeing(IAgent agent, Site centerSite)
        {
            var commonPool = _siteList.AdjacentSites(centerSite).Where(s => s.IsOccupied).ToArray();

            int commonPoolC = commonPool.Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.AgentE] - agent[Agent.VariablesUsedInCode.AgentC]
                + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)commonPool.Length + 1);
        }

        private double CalculatePoolWellbeing(IAgent agent)
        {
            int commonPoolC = agent.ConnectedAgents.Sum(a => a[Agent.VariablesUsedInCode.AgentC]) + agent[Agent.VariablesUsedInCode.AgentC];

            return agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * commonPoolC / ((double)agent.ConnectedAgents.Count + 1);
        }

        private void LookingForBetterSites(IAgent[] orderedAgent)
        {
            List<Site> vacantSites = _siteList.AsSiteEnumerable().Where(s => s.IsOccupied == false).ToList();

            orderedAgent.ForEach(agent =>
            {

                Site[] betterSites = vacantSites.AsParallel()
                        .Select(site => new
                        {
                            site,
                            Wellbeing = CalculateAgentWellbeing(agent, site)
                        })
                        .Where(obj => obj.Wellbeing > agent[Agent.VariablesUsedInCode.AgentSiteWellbeing]).AsSequential()
                        .GroupBy(obj => obj.Wellbeing).OrderByDescending(obj => obj.Key)
                        .Take(1).SelectMany(g => g.Select(o => o.site)).ToArray();

                if (betterSites.Length > 0)
                {
                    agent[Agent.VariablesUsedInCode.AgentBetterSiteAvailable] = true;

                    Site currentSite = agent[Agent.VariablesUsedInCode.AgentCurrentSite];
                    Site selectedSite = betterSites.RandomizeOne();
                    agent[Agent.VariablesUsedInCode.AgentBetterSite] = selectedSite;

                    vacantSites.Add(currentSite);
                    vacantSites.Remove(selectedSite);
                };
            });
        }

        private void UpdateEndowment()
        {
            IAgent agent = _agentList.Agents.First();

            int totalE = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active")
                .Sum(a => (int)a[Agent.VariablesUsedInCode.AgentE]);

            //E(t) == ( r ^ p ) * ( E(t-1) – total_e(t-1) )
            int endowment = Math.Pow(agent[Agent.VariablesUsedInCode.R], agent[Agent.VariablesUsedInCode.P]) * (agent[Agent.VariablesUsedInCode.Endowment] - agent[Agent.VariablesUsedInCode.TotalEndowment]);

            agent.SetToCommon(Agent.VariablesUsedInCode.TotalEndowment, totalE);
            agent.SetToCommon(Agent.VariablesUsedInCode.Endowment, endowment);
        }

    }







    //public sealed class CL3M10Algorithm : IAlgorithm
    //{
    //    public string Name { get { return "Cognitive level 3 Model 10"; } }

    //    string _outputFolder;

    //    AgentList _agentList;

    //    Configuration<CL3M10Agent> _configuration;

    //    ProcessConfiguration _processConfig;

    //    LinkedList<Dictionary<IConfigurableAgent, AgentState>> _iterations = new LinkedList<Dictionary<IConfigurableAgent, AgentState>>();

    //    List<AgentContributionsOutput> _agentContributionsStatistic;
    //    List<RuleFrequenciesOutput> _ruleFrequenciesStatistic;


    //    AnticipatoryLearning al = new AnticipatoryLearning();
    //    ActionSelection acts = new ActionSelection();
    //    ActionTaking at = new ActionTaking();

    //    SocialLearning sl = new SocialLearning();

    //    public CL3M10Algorithm(Configuration<CL3M10Agent> configuration)
    //    {
    //        _configuration = configuration;

    //        _processConfig = new ProcessConfiguration
    //        {
    //            ActionTakingEnabled = true,
    //            AnticipatoryLearningEnabled = true,
    //            RuleSelectionEnabled = true,
    //            RuleSelectionPart2Enabled = true,
    //            SocialLearningEnabled = true
    //        };

    //        _agentContributionsStatistic = new List<AgentContributionsOutput>(_configuration.AlgorithmConfiguration.IterationCount);
    //        _ruleFrequenciesStatistic = new List<RuleFrequenciesOutput>(_configuration.AlgorithmConfiguration.IterationCount);


    //        _outputFolder = @"Output\CL3_M10";

    //        if (Directory.Exists(_outputFolder) == false)
    //            Directory.CreateDirectory(_outputFolder);
    //    }



    //    private void Initialize()
    //    {
    //        _agentList = AgentList.Generate2(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AgentConfiguration, _configuration.InitialState);
    //    }

    //    private void ExecuteAlgorithm()
    //    {
    //        throw new NotImplementedException();
    //    }


    //    private double CalculateAgentWellbeing(IAgent agent)
    //    {
    //        return agent[Agent.VariablesUsedInCode.Engage] - agent[Agent.VariablesUsedInCode.AgentC]
    //            + agent[Agent.VariablesUsedInCode.MagnitudeOfExternalities] * _agentList.CalculateCommonC() / (double)_agentList.Agents.Count;
    //    }

    //    private double CalculateCommonPoolWellbeing(double externalities)
    //    {
    //        return externalities * _agentList.CalculateCommonC() / (double)_agentList.Agents.Count;
    //    }

    //    public string Run()
    //    {
    //        Initialize();


    //        for (int i = 0; i < _configuration.AlgorithmConfiguration.IterationCount; i++)
    //        {
    //            Dictionary<IConfigurableAgent, AgentState> currentIteration;

    //            if (i > 0)
    //                currentIteration = _iterations.AddLast(new Dictionary<IConfigurableAgent, AgentState>()).Value;
    //            else
    //            {
    //                currentIteration = _iterations.AddLast(IterationHelper.InitilizeBeginningState(_configuration.InitialState, _agentList.Agents.Cast<IConfigurableAgent>())).Value;
    //            }

    //            Dictionary<IConfigurableAgent, AgentState> priorIteration = _iterations.Last.Previous?.Value;

    //            var agentGroups = _agentList.Agents.Cast<IConfigurableAgent>().GroupBy(a => a[Agent.VariablesUsedInCode.AgentType]);

    //            //rankedGoals is sorted list
    //            Dictionary<IConfigurableAgent, Goal[]> rankedGoals = new Dictionary<IConfigurableAgent, Goal[]>(_agentList.Agents.Count);

    //            _agentList.Agents.Cast<IConfigurableAgent>().ForEach(a =>
    //            {
    //                a[Agent.VariablesUsedInCode.Iteration] = i + 1;

    //                rankedGoals.Add(a, null);

    //                if (i > 0)
    //                    currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
    //            });

    //            if (_processConfig.AnticipatoryLearningEnabled && i > 0)
    //            {
    //                //1st round: AL, CT, IR
    //                foreach (var agentGroup in agentGroups)
    //                {
    //                    foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
    //                    {
    //                        //Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

    //                        //currentPeriod.SiteStates.Add(actor, new List<SiteState>(assignedSites.Length));



    //                        rankedGoals[agent] = al.Execute(agent, _iterations.Last);

    //                        //if (_processConfig.CounterfactualThinkingEnabled == true)
    //                        //{
    //                        //    optimization
    //                        //    if (rankedGoals[agent].Any(gs => gs.Confidence == false))
    //                        //    {
    //                        //        foreach (Site site in assignedSites.Randomize())
    //                        //        {
    //                        //            foreach (var set in actor.AssignedHeuristics.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
    //                        //            {
    //                        //                //optimization
    //                        //                GoalState criticalGoalState = rankedGoals[actor].First(gs => set.Key.AssociatedWith.Contains(gs.Goal));

    //                        //                if (criticalGoalState.Confidence == false)
    //                        //                {
    //                        //                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
    //                        //                    {
    //                        //                        //optimization
    //                        //                        if (layer.Key.LayerParameters.Modifiable)
    //                        //                        {
    //                        //                            Heuristic[] matchedPriorPeriodHeuristics = priorPeriod.GetStateForSite(actor, site)
    //                        //                                    .Matched.Where(h => h.Layer == layer.Key).ToArray();

    //                        //                            bool? CTResult = null;

    //                        //                            if (matchedPriorPeriodHeuristics.Length >= 2)
    //                        //                                counterfactualThinking.Execute(actor, periods.Last, criticalGoalState,
    //                        //                                matchedPriorPeriodHeuristics, site, layer.Key);


    //                        //                            if (_processConfig.InnovationEnabled == true)
    //                        //                            {

    //                        //                                if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
    //                        //                                    inductiveReasoning.Execute(actor, periods.Last, criticalGoalState, site, layer.Key);
    //                        //                            }
    //                        //                        }
    //                        //                    }
    //                        //                }
    //                        //            }
    //                        //        }
    //                        //    }
    //                        //}
    //                    }
    //                }
    //            }

    //            if (_processConfig.SocialLearningEnabled && i > 0)
    //            {
    //                //2nd round: SL
    //                foreach (var agentGroup in agentGroups)
    //                {

    //                    foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
    //                    {
    //                        foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
    //                        {
    //                            foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
    //                            {
    //                                sl.ExecuteSelection(agent, _iterations.Last.Previous.Value[agent], rankedGoals[agent], layer.Key);
    //                            }
    //                        }
    //                    }

    //                    sl.ExecuteLearning(agentGroup.ToArray(), _iterations.Last.Previous.Value);
    //                }

    //            }

    //            if (_processConfig.RuleSelectionEnabled && i > 0)
    //            {
    //                //AS part I
    //                foreach (var agentGroup in agentGroups)
    //                {
    //                    foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
    //                    {
    //                        //range priority of goals by proportion only

    //                        //Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

    //                        //List<SiteState> siteStates = new List<SiteState>(assignedSites.Length);

    //                        //foreach (Site site in assignedSites.Randomize())
    //                        //{
    //                        //    currentPeriod.SiteStates[actor].Add(SiteState.Create(actor.IsSiteSpecific, site));

    //                        foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
    //                        {
    //                            foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
    //                            {
    //                                acts.ExecutePartI(agent, _iterations.Last, rankedGoals[agent], layer.ToArray());
    //                            }
    //                        }
    //                        //}
    //                    }

    //                    //if (actorGroup.Key.Type == 1)
    //                    //{
    //                    //    SetJobAvailableValue(periods.Last.Value);
    //                    //}
    //                }


    //                if (_processConfig.RuleSelectionPart2Enabled)
    //                {

    //                    //4th round: AS part II
    //                    foreach (var agentGroup in agentGroups)
    //                    {
    //                        foreach (IConfigurableAgent agent in agentGroup.Randomize(_processConfig.AgentRandomizationEnabled))
    //                        {

    //                            //Site[] assignedSites = currentPeriod.GetAssignedSites(actor);

    //                            //foreach (Site site in assignedSites.Randomize())
    //                            //{
    //                            foreach (var set in agent.AssignedRules.GroupBy(r => r.Layer.Set).OrderBy(g => g.Key.PositionNumber))
    //                            {
    //                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
    //                                {
    //                                    acts.ExecutePartII(agent, _iterations.Last, rankedGoals[agent], layer.ToArray(), _agentList.Agents.Count);
    //                                }
    //                            }
    //                            //}

    //                        }
    //                    }

    //                }
    //            }

    //            if (_processConfig.ActionTakingEnabled)
    //            {
    //                //5th round: TA
    //                foreach (var agentGroup in agentGroups)
    //                {
    //                    foreach (IConfigurableAgent agent in agentGroup)
    //                    {
    //                        at.Execute(agent, currentIteration[agent]);

    //                        //if (periods.Last.Value.IsOverconsumption)
    //                        //    return periods;
    //                    }
    //                }
    //            }


    //            Calculations();

    //            _agentContributionsStatistic.Add(new AgentContributionsOutput { Iteration = i + 1, AgentContributions = _agentList.Agents.Select(a => (double)a[Agent.VariablesUsedInCode.AgentC]).ToArray() });
    //            _ruleFrequenciesStatistic.Add(CreateRuleFrequenciesRecord(i+1, currentIteration));

    //            //Maintenance();
    //        }


    //        SaveAgentWellbeingStatistic();
    //        SaveRuleFrequenceStatistic();


    //        return _outputFolder;
    //    }

        

    //    void Calculations()
    //    {
    //        double poolWellbeing = CalculateCommonPoolWellbeing(_agentList.Agents.First()[Agent.VariablesUsedInCode.MagnitudeOfExternalities]);

    //        _agentList.Agents.ForEach(a =>
    //        {
    //            a[Agent.VariablesUsedInCode.PoolWellbeing] = poolWellbeing;
    //            a[Agent.VariablesUsedInCode.AgentWellbeing] = CalculateAgentWellbeing(a);
    //        });
    //    }


    //    RuleFrequenciesOutput CreateRuleFrequenciesRecord(int iteration, Dictionary<IConfigurableAgent, AgentState> iterationState)
    //    {
    //        //todo

    //        //List<Rule> allRules = _agentList.Agents.First().Rules;

    //        RuleFrequenceItem[] items = iterationState.SelectMany(kvp => kvp.Value.Activated).GroupBy(r => r.Id)
    //            .Select(g => new RuleFrequenceItem { RuleId = g.Key, Frequence = g.Count() }).ToArray();



    //        return new RuleFrequenciesOutput { Iteration = iteration, RuleFrequencies = items };


    //    }


    //    void SaveAgentWellbeingStatistic()
    //    {
    //        ResultSavingHelper.Save(_agentContributionsStatistic, $@"{_outputFolder}\contributions_statistic.csv");
    //    }


    //    void SaveRuleFrequenceStatistic()
    //    {
    //        ResultSavingHelper.Save(_ruleFrequenciesStatistic, $@"{_outputFolder}\rule_frequencies_statistic.csv");
    //    }

    //}
}
