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

    public abstract class SosielAlgorithm<T> where T: class, ICloneableAgent<T>
    {
        AlgorithmConfiguration _algorithmConfiguration;

        ProcessConfiguration _processConfiguration;

        protected SiteList _siteList;
        protected AgentList _agentList;
        protected LinkedList<Dictionary<IAgent, AgentState>> _iterations = new LinkedList<Dictionary<IAgent, AgentState>>();

        //processes
        AnticipatoryLearning al = new AnticipatoryLearning();
        CounterfactualThinking ct = new CounterfactualThinking();
        Innovation it = new Innovation();
        SocialLearning sl = new SocialLearning();
        ActionSelection acts = new ActionSelection();
        ActionTaking at = new ActionTaking();


        public SosielAlgorithm(AlgorithmConfiguration algorithmConfiguration, ProcessConfiguration processConfiguration)
        {
            _algorithmConfiguration = algorithmConfiguration;
            _processConfiguration = processConfiguration;
        }

        protected abstract void InitializeAgents();


        protected abstract Dictionary<IAgent, AgentState> InitializeFirstIterationState();


        protected virtual void AfterInitialization() { }

        protected virtual void AfterAlgorithmExecuted() { }


        protected virtual void PreIterationCalculations(int iteration) { }

        protected virtual void PreIterationStatistic(int iteration) { }


        protected virtual void PostIterationCalculations(int iteration) { }

        protected virtual void PostIterationStatistic(int iteration) { }

        protected virtual void AgentsDeactivation()
        {

        }

        protected virtual void Reproduction(int minAgentNumber)
        {
            List<IAgent> activeAgents = _agentList.Agents.Cast<IAgent>()
                .Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active" && _siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentCurrentSite])
                   .Where(s => s.IsOccupied).Sum(s => s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) > 0).ToList();

            if (activeAgents.Count == 0)
                return;

            int newAgentCount = minAgentNumber - activeAgents.Count;

            int lastAgentId = _agentList.Agents.Count + 1;

            while (newAgentCount > 0)
            {
                IAgent targetAgent = activeAgents.RandomizeOne();

                IAgent[] poolOfParticipants = _siteList.CommonPool((Site)targetAgent[Agent.VariablesUsedInCode.AgentCurrentSite])
                    .Where(s => s.IsOccupied).Select(s => s.OccupiedBy).ToArray();

                int contributionsAmount = poolOfParticipants.Sum(a => (int)a[Agent.VariablesUsedInCode.AgentC]);

                List<IAgent> vector = new List<IAgent>(100);

                poolOfParticipants.ForEach(a =>
                {
                    int count = Convert.ToInt32(Math.Round(a[Agent.VariablesUsedInCode.AgentC] / (double)contributionsAmount * 100, MidpointRounding.AwayFromZero));

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
                activeAgents.Add(replica);

                _iterations.ForEach(iteration =>
                {
                    AgentState source = iteration[prototype];

                    iteration.Add(replica, source);
                });
            }
        }




        private void Sosiel()
        {
            for (int i = 1; i <= _algorithmConfiguration.IterationCount; i++)
            {
                PreIterationCalculations(i);
                PreIterationStatistic(i);

                Dictionary<IAgent, AgentState> currentIteration;

                if (i > 1)
                    currentIteration = _iterations.AddLast(new Dictionary<IAgent, AgentState>()).Value;
                else
                    currentIteration = _iterations.AddLast(InitializeFirstIterationState()).Value;

                Dictionary<IAgent, AgentState> priorIteration = _iterations.Last.Previous?.Value;

                IAgent[] orderedAgents = _agentList.Agents.Randomize(_processConfiguration.AgentRandomizationEnabled).ToArray();
                var agentGroups = orderedAgents.GroupBy(a => a[Agent.VariablesUsedInCode.AgentType]).ToArray();


                Dictionary<IAgent, Goal[]> rankedGoals = new Dictionary<IAgent, Goal[]>(_agentList.Agents.Count);

                _agentList.Agents.Cast<IAgent>().ForEach(a =>
                {
                    rankedGoals.Add(a, null);

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
                            rankedGoals[agent] = al.Execute(agent, _iterations.Last);

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
                                                if (layer.Key.LayerSettings.Modifiable)
                                                {
                                                    Rule[] matchedPriorPeriodHeuristics = priorIteration[agent]
                                                            .Matched.Where(h => h.Layer == layer.Key).ToArray();

                                                    bool? CTResult = null;

                                                    if (matchedPriorPeriodHeuristics.Length >= 2)
                                                        CTResult = ct.Execute(agent, _iterations.Last, selectedGoal, matchedPriorPeriodHeuristics, layer.Key);


                                                    if (_processConfiguration.InnovationEnabled == true)
                                                    {
                                                        if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
                                                            it.Execute(agent, _iterations.Last, selectedGoal, layer.Key);
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
                                    sl.ExecuteSelection(agent, _iterations.Last.Previous.Value[agent], rankedGoals[agent], layer.Key);
                                }
                            }
                        }

                        sl.ExecuteLearning(agentGroup.ToArray(), _iterations.Last.Previous.Value);
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
                                    acts.ExecutePartI(agent, _iterations.Last, rankedGoals[agent], layer.ToArray());
                                }
                            }

                        }
                    }


                    if (_processConfiguration.RuleSelectionPart2Enabled)
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
                                        acts.ExecutePartII(agent, _iterations.Last, rankedGoals[agent], layer.ToArray(), _agentList.Agents.Count);
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
                            at.Execute(agent, currentIteration[agent]);

                            //if (periods.Last.Value.IsOverconsumption)
                            //    return periods;
                        }
                    }
                }

                PostIterationCalculations(i);

                PostIterationStatistic(i);

                if (_processConfiguration.AgentsDeactivationEnabled)
                {
                    AgentsDeactivation();
                }

                if (_processConfiguration.ReproductionEnabled)
                {
                    Reproduction(_algorithmConfiguration.AgentCount);
                }
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
