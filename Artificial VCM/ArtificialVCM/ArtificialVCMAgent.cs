﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ArtificialVCM.Configuration;
using Common.Entities;
using Common.Exceptions;
using Common.Helpers;
using Common.Randoms;

namespace ArtificialVCM
{
    public sealed class ArtificialVCMAgent : Agent
    {
        public AgentStateConfiguration AgentStateConfiguration { get; private set; }

        public override Agent Clone()
        {
            ArtificialVCMAgent agent = (ArtificialVCMAgent)base.Clone();

            return agent;
        }

        public void GenerateCustomParams()
        {

        }

        /// <summary>
        /// Creates agent instance based on agent prototype and agent configuration 
        /// </summary>
        /// <param name="agentConfiguration"></param>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public static ArtificialVCMAgent CreateAgent(AgentStateConfiguration agentConfiguration, AgentPrototype prototype)
        {
            ArtificialVCMAgent agent = new ArtificialVCMAgent();

            agent.Prototype = prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(agentConfiguration.PrivateVariables);

            agent.AssignedKnowledgeHeuristics = prototype.KnowledgeHeuristics.Where(r => agentConfiguration.AssignedKnowledgeHeuristics.Contains(r.Id)).ToList();
            agent.AssignedGoals = prototype.Goals.Where(g => agentConfiguration.AssignedGoals.Contains(g.Name)).ToList();

            agent.AssignedKnowledgeHeuristics.ForEach(kh => agent.KnowledgeHeuristicActivationFreshness.Add(kh, 1));

            //generate goal importance
            agentConfiguration.GoalsState.ForEach(kvp =>
            {
                var goalName = kvp.Key;
                var configuration = kvp.Value;

                var goal = agent.AssignedGoals.FirstOrDefault(g => g.Name == goalName);
                if (goal == null) return;

                double importance = configuration.Importance;

                if (configuration.Randomness)
                {
                    if (string.IsNullOrEmpty(configuration.BasedOn))
                    {
                        var from = (int)(configuration.RandomFrom * 10);
                        var to = (int)(configuration.RandomTo * 10);

                        importance = GenerateImportance(agent, configuration.RandomFrom, configuration.RandomTo);
                    }
                    else
                    {
                        var anotherGoalImportance = agent.InitialGoalStates[agent.AssignedGoals.FirstOrDefault(g => g.Name == configuration.BasedOn)]
                            .Importance;

                        importance = Math.Round(1 - anotherGoalImportance, 2);
                    }
                }

                GoalState goalState = new GoalState(configuration.Value, goal.FocalValue, importance);

                agent.InitialGoalStates.Add(goal, goalState);

                agent[string.Format("{0}_Importance", goal.Name)] = importance;
            });

            //initializes initial anticipated influence for each kh and goal assigned to the agent
            //agent.AssignedKnowledgeHeuristics.ForEach(kh =>
            //{
            //    Dictionary<string, double> source;

            //    if (kh.AutoGenerated && agent.Prototype.DoNothingAnticipatedInfluence != null)
            //    {
            //        source = agent.Prototype.DoNothingAnticipatedInfluence;
            //    }
            //    else
            //    {
            //        agentConfiguration.AnticipatedInfluenceState.TryGetValue(kh.Id, out source);
            //    }


            //    Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

            //    agent.AssignedGoals.ForEach(g =>
            //    {
            //        inner.Add(g, source != null && source.ContainsKey(g.Name) ? source[g.Name] : 0);
            //    });

            //    agent.AnticipationInfluence.Add(kh, inner);
            //});

            //manually initialize AI
            agent.AssignedKnowledgeHeuristics.ForEach(kh =>
            {
                var goal1 = agent.AssignedGoals.FirstOrDefault(g => g.Name == "G1");
                var goal2 = agent.AssignedGoals.FirstOrDefault(g => g.Name == "G2");

                Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

                if (kh.PositionNumber == 1)
                {
                    if (goal1 != null) inner.Add(goal1, agent[AlgorithmVariables.E]);
                    if (goal2 != null) inner.Add(goal2, agent[AlgorithmVariables.E] - 1);
                }

                if (kh.PositionNumber == 2)
                {
                    if (goal1 != null) inner.Add(goal1, 0);
                    if (goal2 != null) inner.Add(goal2, agent[AlgorithmVariables.E]);
                }

                agent.AnticipationInfluence.Add(kh, inner);
            });

            InitializeDynamicvariables(agent);


            //applying transforms
            if (agentConfiguration.VariablesTransform != null)
            {
                agentConfiguration.VariablesTransform.ForEach(kvp =>
                {
                    var variable = kvp.Key;
                    var reference = kvp.Value;

                    switch (reference)
                    {
                        case AlgorithmVariables.G1Importance:
                            {
                                agent[variable] = agent[AlgorithmVariables.E] * agent[AlgorithmVariables.G1Importance];
                                break;
                            }
                        case AlgorithmVariables.G2Importance:
                            {
                                agent[variable] = agent[AlgorithmVariables.E] * agent[AlgorithmVariables.G2Importance];
                                break;
                            }

                        default:
                            throw new UnknownVariableException(reference);
                    }
                });
            }

            if (agentConfiguration.AnticipatedInfluenceTransform != null)
            {
                agentConfiguration.AnticipatedInfluenceTransform.ForEach(kvp =>
                {
                    var heuristic = agent.AssignedKnowledgeHeuristics.FirstOrDefault(kh => kh.Id == kvp.Key);

                    kvp.Value.ForEach(goalsInfluence =>
                    {
                        var goal = agent.AssignedGoals.FirstOrDefault(g => g.Name == goalsInfluence.Key);
                        agent.AnticipationInfluence[heuristic][goal] = agent[goalsInfluence.Value];
                    });
                });
            }

            agent.AgentStateConfiguration = agentConfiguration;

            return agent;
        }

        private static void InitializeDynamicvariables(ArtificialVCMAgent agent)
        {
            var e = agent[AlgorithmVariables.E];
            var m = agent[AlgorithmVariables.M];
            var importance = agent[AlgorithmVariables.G2Importance];

            agent[AlgorithmVariables.AgentProfit] = e * (1 - importance * (1 - m));
            agent[AlgorithmVariables.AgentC] = e * importance;
            agent[AlgorithmVariables.CommonProfit] = e * importance;
        }

        private static double GenerateImportance(ArtificialVCMAgent agent, double min, double max)
        {
            double rand;

            if (agent.ContainsVariable(AlgorithmVariables.Mean) && agent.ContainsVariable(AlgorithmVariables.StdDev))
                rand = NormalDistributionRandom.GetInstance.Next(agent[AlgorithmVariables.Mean], agent[AlgorithmVariables.StdDev]);
            else
                rand = NormalDistributionRandom.GetInstance.Next();

            rand = Math.Round(rand, 1, MidpointRounding.AwayFromZero);

            if (rand < min || rand > max)
                return GenerateImportance(agent, min, max);

            return rand;
        }
    }
}
