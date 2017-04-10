using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Algorithm
{
    using Configuration;
    using Entities;
    using Helpers;
    using Processes;
    using Exceptions;

    public abstract class SosielAlgorithm<T> where T : class, ICloneableAgent<T>
    {
        AlgorithmConfiguration _algorithmConfiguration;

        ProcessConfiguration _processConfiguration;

        protected bool _algorithmStoppage = false;
        protected int _numberOfAgents = 0;
        protected SiteList _siteList;
        protected AgentList _agentList;
        protected LinkedList<Dictionary<IAgent, AgentState>> _iterations = new LinkedList<Dictionary<IAgent, AgentState>>();
        protected Dictionary<string, Action<IAgent>> _preliminaryCalculations = new Dictionary<string, Action<IAgent>>();

        //processes
        AnticipatoryLearning _al = new AnticipatoryLearning();
        CounterfactualThinking _ct = new CounterfactualThinking();
        Innovation _it = new Innovation();
        SocialLearning _sl = new SocialLearning();
        ActionSelection _acts = new ActionSelection();
        ActionTaking _at = new ActionTaking();


        

        public SosielAlgorithm(AlgorithmConfiguration algorithmConfiguration, ProcessConfiguration processConfiguration)
        {
            _algorithmConfiguration = algorithmConfiguration;
            _processConfiguration = processConfiguration;
        }

        protected abstract void InitializeAgents();


        protected abstract Dictionary<IAgent, AgentState> InitializeFirstIterationState();


        protected virtual void AfterInitialization() { }

        protected virtual void AfterAlgorithmExecuted() { }


        protected virtual void PreIterationCalculations(int iteration, IAgent[] orderedAgents) { }

        protected virtual void PreIterationStatistic(int iteration) { }


        protected virtual void PostIterationCalculations(int iteration, IAgent[] orderedAgents) { }

        protected virtual void PostIterationStatistic(int iteration) { }

        protected virtual void AgentsDeactivation()
        {

        }

        protected virtual void Reproduction(int minAgentNumber)
        {
            IAgent[] activeAgents = _agentList.ActiveAgents;

            //Use a uniform distribution to select an agent, agent(i), with: (a) “active” status, (b) at least one neighbor, and (c) common_pool(i)_c(t) > 0.
            List<IAgent> suitableAgents = activeAgents.Where(a => a[Agent.VariablesUsedInCode.AgentC] > 0 
                || a.ConnectedAgents.Any(n=>n[Agent.VariablesUsedInCode.AgentStatus] == "active" && n[Agent.VariablesUsedInCode.AgentC] > 0)).ToList();

            if (suitableAgents.Count == 0)
                return;

            int newAgentCount = minAgentNumber - activeAgents.Length;

            int lastAgentId = _agentList.Agents.Count + 1;

            while (newAgentCount > 0)
            {
                IAgent targetAgent = suitableAgents.RandomizeOne();

                List<IAgent> poolOfParticipants = targetAgent.ConnectedAgents.Where(a=> a[Agent.VariablesUsedInCode.AgentStatus] == "active").ToList();
                poolOfParticipants.Add(targetAgent);

                double contributions = poolOfParticipants.Sum(a => (int)a[Agent.VariablesUsedInCode.AgentC]);

                List<IAgent> vector = new List<IAgent>(100);

                poolOfParticipants.ForEach(a =>
                {
                    int count = Convert.ToInt32(Math.Round(a[Agent.VariablesUsedInCode.AgentC] / contributions * 100, MidpointRounding.AwayFromZero));

                    for (int i = 0; i < count; i++) { vector.Add(a); }
                });

                T prototype = vector.RandomizeOne() as T;

                IAgent replica = prototype.Clone();

                replica.Id = lastAgentId;

                lastAgentId++;

                Site targetSite = _siteList.TakeClosestEmptySites((Site)targetAgent[Agent.VariablesUsedInCode.AgentCurrentSite]).RandomizeOne();

                replica[Agent.VariablesUsedInCode.AgentCurrentSite] = targetSite;

                newAgentCount--;

                _agentList.Agents.Add(replica);
                suitableAgents.Add(replica);

                _iterations.ForEach(iteration =>
                {
                    AgentState source = iteration[prototype];

                    iteration.Add(replica, source);
                });
            }
        }




        private void Sosiel()
        {
            for (int i = 1; i <= _algorithmConfiguration.NumberOfIterations; i++)
            {
                Console.WriteLine($"Starting {i} iteration");

                IAgent[] orderedAgents = _agentList.ActiveAgents.Randomize(_processConfiguration.AgentRandomizationEnabled).ToArray();
                var agentGroups = orderedAgents.GroupBy(a => a[Agent.VariablesUsedInCode.AgentType]).ToArray();

                PreIterationCalculations(i, orderedAgents);
                PreIterationStatistic(i);

                Dictionary<IAgent, AgentState> currentIteration;

                if (i > 1)
                    currentIteration = _iterations.AddLast(new Dictionary<IAgent, AgentState>()).Value;
                else
                    currentIteration = _iterations.AddLast(InitializeFirstIterationState()).Value;

                Dictionary<IAgent, AgentState> priorIteration = _iterations.Last.Previous?.Value;



                Dictionary<IAgent, Goal[]> rankedGoals = new Dictionary<IAgent, Goal[]>(_agentList.Agents.Count);

                orderedAgents.ForEach(a =>
                {
                    rankedGoals.Add(a, a.Goals.ToArray());

                    if (i > 1)
                        currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
                });


                if (_processConfiguration.AnticipatoryLearningEnabled && i > 1)
                {
                    //1st round: AL, CT, IR
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {
                            //Anticipatory Learning Process
                            rankedGoals[agent] = _al.Execute(agent, _iterations.Last);

                            if (_processConfiguration.CounterfactualThinkingEnabled == true)
                            {
                                if (rankedGoals[agent].Any(g => currentIteration[agent].GoalsState.Any(kvp => kvp.Value.Confidence == false)))
                                {
                                    foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        //optimization
                                        Goal selectedGoal = rankedGoals[agent].First(g => set.Key.AssociatedWith.Contains(g));

                                        GoalState selectedGoalState = currentIteration[agent].GoalsState[selectedGoal];

                                        if (selectedGoalState.Confidence == false)
                                        {
                                            foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                            {
                                                if (layer.Key.LayerSettings.Modifiable || (!layer.Key.LayerSettings.Modifiable && layer.Any(r => r.IsModifiable)))
                                                {
                                                    Rule[] matchedPriorPeriodHeuristics = priorIteration[agent]
                                                            .Matched.Where(h => h.Layer == layer.Key).ToArray();

                                                    bool? CTResult = null;

                                                    if (matchedPriorPeriodHeuristics.Length >= 2)
                                                        CTResult = _ct.Execute(agent, _iterations.Last, selectedGoal, matchedPriorPeriodHeuristics, layer.Key);


                                                    if (_processConfiguration.InnovationEnabled == true)
                                                    {
                                                        if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
                                                            _it.Execute(agent, _iterations.Last, selectedGoal, layer.Key);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (_processConfiguration.SocialLearningEnabled && i > 1)
                {
                    //2nd round: SL
                    foreach (var agentGroup in agentGroups)
                    {

                        foreach (IAgent agent in agentGroup)
                        {
                            foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    _sl.ExecuteSelection(agent, _iterations.Last.Previous.Value[agent], rankedGoals[agent], layer.Key);
                                }
                            }
                        }

                        _sl.ExecuteLearning(agentGroup.ToArray(), _iterations.Last.Previous.Value);
                    }

                }

                if (_processConfiguration.RuleSelectionEnabled && i > 1)
                {
                    //AS part I
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {

                            foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    string preliminaryCalculationsMethodName = layer.Key.LayerSettings.PreliminaryСalculations;

                                    if (string.IsNullOrEmpty(preliminaryCalculationsMethodName) == false)
                                    {
                                        try
                                        {
                                            Action<IAgent> method = _preliminaryCalculations[preliminaryCalculationsMethodName];

                                            method(agent);
                                        }
                                        catch (KeyNotFoundException)
                                        {
                                            throw new AlgorithmException($"Preliminary calculation: {preliminaryCalculationsMethodName} hasn't implemented in current model");
                                        }
                                    }

                                    _acts.ExecutePartI(agent, _iterations.Last, rankedGoals[agent], layer.ToArray());
                                }

                                if (set.Key.IsSequential)
                                {
                                    _at.ExecuteForSpecificRuleSet(agent, currentIteration[agent], set.Key);

                                    PostIterationCalculations(i, orderedAgents);
                                }
                            }
                        }
                    }


                    if (_processConfiguration.RuleSelectionPart2Enabled && i > 1)
                    {
                        //4th round: AS part II
                        foreach (var agentGroup in agentGroups)
                        {
                            foreach (IAgent agent in agentGroup)
                            {

                                foreach (var set in agent.AssignedRules.GroupBy(r => r.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        _acts.ExecutePartII(agent, _iterations.Last, rankedGoals[agent], layer.ToArray(), orderedAgents);
                                    }
                                }
                            }
                        }
                    }
                }

                if (_processConfiguration.ActionTakingEnabled)
                {
                    //5th round: TA
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {
                            _at.Execute(agent, currentIteration[agent]);

                            //if (periods.Last.Value.IsOverconsumption)
                            //    return periods;
                        }
                    }
                }

                if (_processConfiguration.AlgorithmStopIfAllAgentsSelectDoNothing && i > 1)
                {
                    if (currentIteration.SelectMany(kvp => kvp.Value.Activated).All(r => r.IsAction == false))
                    {
                        _algorithmStoppage = true;
                    }
                }

                PostIterationCalculations(i, orderedAgents);

                PostIterationStatistic(i);

                if (_processConfiguration.AgentsDeactivationEnabled && i > 1)
                {
                    AgentsDeactivation();
                }

                if (_processConfiguration.ReproductionEnabled)
                {
                    Reproduction(_numberOfAgents);
                }

                if (_algorithmStoppage || _agentList.ActiveAgents.Length == 0)
                    break;
            }
        }

        protected void ExecuteAlgorithm()
        {
            InitializeAgents();

            AfterInitialization();

            Sosiel();

            AfterAlgorithmExecuted();
        }
    }
}
