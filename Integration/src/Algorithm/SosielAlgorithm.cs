using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Algorithm
{
	using Configuration;
	using Entities;
	using Helpers;
	using Processes;
	using Exceptions;

	public abstract class SosielAlgorithm
	{
		private int numberOfIterations;
		private ProcessesConfiguration processConfiguration;

        protected int numberOfAgentsAfterInitialize; 
		protected bool algorithmStoppage = false;
		protected AgentList agentList;
		protected LinkedList<Dictionary<IAgent, AgentState>> iterations = new LinkedList<Dictionary<IAgent, AgentState>>();

		protected Dictionary<string, Action<IAgent>> preliminaryCalculations = new Dictionary<string, Action<IAgent>>();

		//processes
		AnticipatoryLearning al = new AnticipatoryLearning();
		CounterfactualThinking ct = new CounterfactualThinking();
		Innovation it = new Innovation();
		SocialLearning sl = new SocialLearning();
		ActionSelection acts = new ActionSelection();
		ActionTaking at = new ActionTaking();


		public SosielAlgorithm(int numberOfIterations, ProcessesConfiguration processConfiguration)
		{
			this.numberOfIterations = numberOfIterations;
			this.processConfiguration = processConfiguration;
		}

		protected abstract void InitializeAgents();


		protected abstract Dictionary<IAgent, AgentState> InitializeFirstIterationState();


		protected virtual void AfterInitialization() { }

		protected virtual void AfterAlgorithmExecuted() { }


		protected virtual void PreIterationCalculations(int iteration, IAgent[] orderedAgents) { }

		protected virtual void PreIterationStatistic(int iteration) { }


		protected virtual void PostIterationCalculations(int iteration, IAgent[] orderedAgents) { }

		protected virtual void PostIterationStatistic(int iteration) { }

		protected virtual void AgentsDeactivation() { }

		protected virtual void AfterDeactivation(int iteration) { }



		protected virtual void Reproduction(int minAgentNumber)
		{
			
		}

        protected virtual void Maintenance()
        {
            agentList.ActiveAgents.ForEach(a =>
            {
                a.RuleActivationFreshness.Keys.ToArray().ForEach(k =>
                {
                    a.RuleActivationFreshness[k] += 1;
                });
            });
        }


        private void RunSosiel()
		{
			for (int i = 1; i <= numberOfIterations; i++)
			{
				Console.WriteLine($"Starting {i} iteration");

				IAgent[] orderedAgents = agentList.ActiveAgents.Randomize(processConfiguration.AgentRandomizationEnabled).ToArray();

				var agentGroups = orderedAgents.GroupBy(a => a[VariablesUsedInCode.AgentType]).OrderBy(group => group.Key).ToArray();

				PreIterationCalculations(i, orderedAgents);
				PreIterationStatistic(i);

				Dictionary<IAgent, AgentState> currentIteration;

				if (i > 1)
					currentIteration = iterations.AddLast(new Dictionary<IAgent, AgentState>()).Value;
				else
					currentIteration = iterations.AddLast(InitializeFirstIterationState()).Value;

				Dictionary<IAgent, AgentState> priorIteration = iterations.Last.Previous?.Value;



				Dictionary<IAgent, Goal[]> rankedGoals = new Dictionary<IAgent, Goal[]>(agentList.Agents.Count);

				orderedAgents.ForEach(a =>
				{
					rankedGoals.Add(a, a.AssignedGoals.ToArray());

					if (i > 1)
						currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
				});


				if (processConfiguration.AnticipatoryLearningEnabled && i > 1)
				{
					//1st round: AL, CT, IR
					foreach (var agentGroup in agentGroups)
					{
						foreach (IAgent agent in agentGroup)
						{
							//Anticipatory Learning Process
							rankedGoals[agent] = al.Execute(agent, iterations.Last);

							if (processConfiguration.CounterfactualThinkingEnabled == true)
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
														CTResult = ct.Execute(agent, iterations.Last, selectedGoal, matchedPriorPeriodHeuristics, layer.Key);


													if (processConfiguration.InnovationEnabled == true)
													{
														if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)
															it.Execute(agent, iterations.Last, selectedGoal, layer.Key);
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

				if (processConfiguration.SocialLearningEnabled && i > 1)
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
									sl.ExecuteSelection(agent, currentIteration[agent], priorIteration[agent], layer.Key);
								}
							}
						}

						sl.ExecuteLearning(agentGroup.ToArray(), currentIteration, rankedGoals);
					}

				}

				if (processConfiguration.RuleSelectionEnabled && i > 1)
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
											Action<IAgent> method = preliminaryCalculations[preliminaryCalculationsMethodName];

											method(agent);
										}
										catch (KeyNotFoundException)
										{
											throw new SosielAlgorithmException($"Preliminary calculation: {preliminaryCalculationsMethodName} hasn't implemented in current model");
										}
									}

									acts.ExecutePartI(agent, iterations.Last, rankedGoals[agent], layer.ToArray());
								}

								if (set.Key.IsSequential)
								{
									at.ExecuteForSpecificRuleSet(agent, currentIteration[agent], set.Key);

									PostIterationCalculations(i, orderedAgents);
								}
							}
						}
					}


					if (processConfiguration.RuleSelectionPart2Enabled && i > 1)
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
										acts.ExecutePartII(agent, iterations.Last, rankedGoals[agent], layer.ToArray(), orderedAgents);
									}
								}
							}
						}
					}
				}

				if (processConfiguration.ActionTakingEnabled)
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

				if (processConfiguration.AlgorithmStopIfAllAgentsSelectDoNothing && i > 1)
				{
					if (currentIteration.SelectMany(kvp => kvp.Value.Activated).All(r => r.IsAction == false))
					{
						algorithmStoppage = true;
					}
				}

				PostIterationCalculations(i, orderedAgents);

				PostIterationStatistic(i);

				if (processConfiguration.AgentsDeactivationEnabled && i > 1)
				{
					AgentsDeactivation();
				}

				AfterDeactivation(i);

                if (processConfiguration.ReproductionEnabled && i > 1)
                {
                    Reproduction(0);
                }

                if (algorithmStoppage || agentList.ActiveAgents.Length == 0)
					break;

				Maintenance();
			}
		}
	}
}
