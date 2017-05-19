﻿using System;
using System.Collections.Generic;
using System.Linq;

using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Enums;
    using Entities;
    using Randoms;

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
            RuleLayer layer, ActiveSite site)
        {
            Dictionary<IAgent, AgentState> currentIteration = lastIteration.Value;
            Dictionary<IAgent, AgentState> priorIteration = lastIteration.Previous.Value;

            //gets prior period activated rule
            RuleHistory history = priorIteration[agent].RuleHistories[site];
            Rule priorPeriodRule = history.Activated.Single(r=>r.Layer == layer);

            LinkedListNode<Dictionary<IAgent, AgentState>> tempNode = lastIteration.Previous;

            //if prior period rule is do nothing rule then looking for any do something rule
            while (priorPeriodRule.IsAction == false && tempNode.Previous != null)
            {
                tempNode = tempNode.Previous;

                history = tempNode.Value[agent].RuleHistories[site];

                priorPeriodRule = history.Activated.Single(r => r.Layer == layer);
            }

            //if the layer or prior period rule are modifiable then generate new rule
            if (layer.LayerSettings.Modifiable || (!layer.LayerSettings.Modifiable && priorPeriodRule.IsModifiable))
            {
                RuleLayerSettings parameters = layer.LayerSettings;

                Goal selectedGoal = goal;

                GoalState selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

                #region Generating consequent
                double min = parameters.MinValue(agent);
                double max = parameters.MaxValue(agent);

                double consequentValue = priorPeriodRule.Consequent.Value;

                switch (selectedGoalState.AnticipatedDirection)
                {
                    case AnticipatedDirection.Up:
                        {
                            if (RuleLayerSettings.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                max = Math.Abs(consequentValue - max);

                                consequentValue += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }
                            if (RuleLayerSettings.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                max = Math.Abs(consequentValue - min);

                                consequentValue -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }

                            break;
                        }
                    case AnticipatedDirection.Down:
                        {
                            if (RuleLayerSettings.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                max = Math.Abs(consequentValue - min);

                                consequentValue -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }
                            if (RuleLayerSettings.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                max = Math.Abs(consequentValue - max);

                                consequentValue += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                            }

                            break;
                        }
                    default:
                        {
                            throw new Exception("Not implemented for AnticipatedDirection == 'stay'");
                        }
                }

                RuleConsequent consequent = RuleConsequent.Renew(priorPeriodRule.Consequent, consequentValue);
                #endregion


                #region Generating antecedent
                List<RuleAntecedentPart> antecedentList = new List<RuleAntecedentPart>(priorPeriodRule.Antecedent.Length);

                foreach (RuleAntecedentPart antecedent in priorPeriodRule.Antecedent)
                {
                    dynamic newConst = agent[antecedent.Param];

                    RuleAntecedentPart newAntecedent = RuleAntecedentPart.Renew(antecedent, newConst);

                    antecedentList.Add(newAntecedent);
                }
                #endregion

                AgentState agentState = currentIteration[agent];

                Rule generatedRule = priorPeriodRule.Copy();
                generatedRule.Antecedent = antecedentList.ToArray();
                generatedRule.Consequent = consequent;

                //add the generated rule to the prototype's mental model and assign one to the agent's mental model 
                agent.AddRule(generatedRule, layer, agent.AnticipationInfluence[priorPeriodRule]);

                if (layer.Set.Layers.Count > 1)
                    //set consequent to actor's variables for next layers
                    generatedRule.Apply(agent);
            }
        }
    }
}