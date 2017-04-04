using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;
    using Randoms;

    public class Innovation
    {
        public void Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Goal goal,
            RuleLayer layer)
        {
            Dictionary<IAgent, AgentState> currentIteration = lastIteration.Value;
            Dictionary<IAgent, AgentState> priorIteration = lastIteration.Previous.Value;

            Rule priorPeriodRule = priorIteration[agent].Activated.Single(r=>r.Layer == layer);

            LinkedListNode<Dictionary<IAgent, AgentState>> tempNode = lastIteration.Previous;

            while (priorPeriodRule.IsAction == false && tempNode.Previous != null)
            {
                tempNode = tempNode.Previous;
                priorPeriodRule = tempNode.Value[agent].Activated.Single(r => r.Layer == layer);
            }

            RuleLayerSettings parameters = layer.LayerSettings;

            Goal selectedGoal = goal;

            GoalState selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

            #region Generating consequent
            double min = parameters.MinValue;
            double max = parameters.MaxValue;

            double consequentValue = priorPeriodRule.Consequent.Value;

            switch (selectedGoalState.AnticipatedDirection)
            {
                case AnticipatedDirection.Up:
                    {
                        if (parameters.ConsequentRelationship == ConsequentRelationship.Positive)
                        {
                            max = Math.Abs(consequentValue - max);

                            consequentValue += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }
                        if (parameters.ConsequentRelationship == ConsequentRelationship.Negative)
                        {
                            max = Math.Abs(consequentValue - min);

                            consequentValue -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }

                        break;
                    }
                case AnticipatedDirection.Down:
                    {
                        if (parameters.ConsequentRelationship == ConsequentRelationship.Positive)
                        {
                            max = Math.Abs(consequentValue - min);

                            consequentValue -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }
                        if (parameters.ConsequentRelationship == ConsequentRelationship.Negative)
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
            List<RuleAntecedentPart> antecedentsList = new List<RuleAntecedentPart>(priorPeriodRule.Antecedent.Length);
            
            foreach(RuleAntecedentPart antecedent in priorPeriodRule.Antecedent)
            {
                dynamic newConst = agent[antecedent.Param];

                RuleAntecedentPart newAntecedent = RuleAntecedentPart.Renew(antecedent, newConst);

                antecedentsList.Add(newAntecedent);
            }
            #endregion

            Rule generatedRule = Rule.Create(antecedentsList.ToArray(), consequent);

            layer.Add(generatedRule);

            agent.AssignedRules.Add(generatedRule);

            if(layer.Set.Layers.Count > 1)
                //set consequent to actor's variables for next layers
                generatedRule.Apply(agent);
        }
    }
}