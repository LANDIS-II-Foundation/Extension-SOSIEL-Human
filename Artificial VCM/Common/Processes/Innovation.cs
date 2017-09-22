using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;
    using Helpers;
    using Randoms;

    /// <summary>
    /// Innovation process implementation.
    /// </summary>
    public class Innovation
    {
        /// <summary>
        /// Executes agent innovation process for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="goal"></param>
        /// <param name="layer"></param>
        public void Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Goal goal,
            KnowledgeHeuristicsLayer layer, Site site)
        {
            Dictionary<IAgent, AgentState> currentIteration = lastIteration.Value;
            Dictionary<IAgent, AgentState> priorIteration = lastIteration.Previous.Value;

            //gets prior period activated heuristic
            KnowledgeHeuristicsHistory history = priorIteration[agent].HeuristicHistories[site];
            KnowledgeHeuristic priorPeriodHeuristic = history.Activated.Single(r=>r.Layer == layer);

            LinkedListNode<Dictionary<IAgent, AgentState>> tempNode = lastIteration.Previous;

            //if prior period heuristic is do nothing heuristic then looking for any do something heuristic
            while (priorPeriodHeuristic.IsAction == false && tempNode.Previous != null)
            {
                tempNode = tempNode.Previous;

                history = tempNode.Value[agent].HeuristicHistories[site];

                priorPeriodHeuristic = history.Activated.Single(r => r.Layer == layer);
            }

            //if the layer or prior period heuristic are modifiable then generate new heuristic
            if (layer.LayerConfiguration.Modifiable || (!layer.LayerConfiguration.Modifiable && priorPeriodHeuristic.IsModifiable))
            {
                KnowledgeHeuristicsLayerConfiguration parameters = layer.LayerConfiguration;

                Goal selectedGoal = goal;

                GoalState selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

                #region Generating consequent
                double min = parameters.MinValue(agent);
                double max = parameters.MaxValue(agent);

                double consequentValue = string.IsNullOrEmpty(priorPeriodHeuristic.Consequent.VariableValue)
                    ? priorPeriodHeuristic.Consequent.Value
                    : agent[priorPeriodHeuristic.Consequent.VariableValue];

                double newConsequent = consequentValue;

                switch (selectedGoalState.AnticipatedDirection)
                {
                    case AnticipatedDirection.Up:
                        {
                            if (KnowledgeHeuristicsLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                max = Math.Abs(consequentValue - max);

                                newConsequent += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }
                            if (KnowledgeHeuristicsLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                max = Math.Abs(consequentValue - min);

                                newConsequent -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }

                            break;
                        }
                    case AnticipatedDirection.Down:
                        {
                            if (KnowledgeHeuristicsLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                max = Math.Abs(consequentValue - min);

                                newConsequent -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }
                            if (KnowledgeHeuristicsLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                max = Math.Abs(consequentValue - max);

                                newConsequent += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }

                            break;
                        }
                    default:
                        {
                            throw new Exception("Not implemented for AnticipatedDirection == 'stay'");
                        }
                }

                KnowledgeHeuristicConsequent consequent = KnowledgeHeuristicConsequent.Renew(priorPeriodHeuristic.Consequent, newConsequent);
                #endregion


                #region Generating antecedent
                List<KnowledgeHeuristicAntecedentPart> antecedentList = new List<KnowledgeHeuristicAntecedentPart>(priorPeriodHeuristic.Antecedent.Length);

                foreach (KnowledgeHeuristicAntecedentPart antecedent in priorPeriodHeuristic.Antecedent)
                {
                    dynamic newConst = agent[antecedent.Param];

                    KnowledgeHeuristicAntecedentPart newAntecedent = KnowledgeHeuristicAntecedentPart.Renew(antecedent, newConst);

                    antecedentList.Add(newAntecedent);
                }
                #endregion

                AgentState agentState = currentIteration[agent];

                KnowledgeHeuristic generatedHeuristic = KnowledgeHeuristic.Renew(priorPeriodHeuristic, antecedentList.ToArray(), consequent);


                //change base ai values for the new heuristic
                double consequentChangeProportion;
                if (consequentValue == 0)
                {
                    consequentChangeProportion = 0;
                }
                else
                {
                    consequentChangeProportion = Math.Abs(generatedHeuristic.Consequent.Value - consequentValue) / consequentValue;
                }

                
                Dictionary<Goal, double> baseAI = agent.AnticipationInfluence[priorPeriodHeuristic];

                Dictionary<Goal, double> proportionalAI = new Dictionary<Goal, double>();



                agent.AssignedGoals.ForEach(g =>
                {
                    double ai = baseAI[g];

                    ConsequentRelationship relationship = KnowledgeHeuristicsLayerConfiguration.ConvertSign(priorPeriodHeuristic.Layer.LayerConfiguration.ConsequentRelationshipSign[g.Name]);

                    double difference = Math.Abs(ai * consequentChangeProportion);


                    switch(selectedGoalState.AnticipatedDirection)
                    {
                        case AnticipatedDirection.Up:
                            {
                                if (relationship == ConsequentRelationship.Positive)
                                {
                                    ai -= difference;
                                }
                                else
                                {
                                    ai += difference;
                                }

                                break;
                            }
                        case AnticipatedDirection.Down:
                            {
                                if (relationship == ConsequentRelationship.Positive)
                                {
                                    ai += difference;
                                }
                                else
                                {
                                    ai -= difference;
                                }

                                break;
                            }
                    }

                    proportionalAI.Add(g, ai);
                });


                //add the generated heuristic to the prototype's mental model and assign one to the agent's mental model 

                if (agent.Prototype.IsSimilarHeuristicExists(generatedHeuristic) == false)
                {
                    //add to the prototype and assign to current agent
                    agent.AddHeuristic(generatedHeuristic, layer, proportionalAI);
                }
                else if(agent.AssignedKnowledgeHeuristics.Any(heuristic => heuristic == generatedHeuristic) == false)
                {
                    //assign to current agent only
                    agent.AssignNewHeuristic(generatedHeuristic, proportionalAI);
                }

                if (layer.Set.Layers.Count > 1)
                    //set consequent to actor's variables for next layers
                    generatedHeuristic.Apply(agent);
            }
        }
    }
}