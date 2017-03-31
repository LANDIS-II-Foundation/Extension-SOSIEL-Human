using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Models;
    using Randoms;

    class Innovation
    {
        public void Execute(Actor actor, LinkedListNode<Period> periodModel, GoalState goalState, Site site, HeuristicLayer layer)
        {
            Period currentPeriod = periodModel.Value;
            Period priorPeriod = periodModel.Previous.Value;

            Heuristic activatedPriorPeriodHeuristic = priorPeriod.GetStateForSite(actor, site).GetActivated(layer);

            LinkedListNode<Period> tempNode = periodModel.Previous;

            while (activatedPriorPeriodHeuristic.IsAction == false && tempNode.Previous != null)
            {
                tempNode = tempNode.Previous;
                activatedPriorPeriodHeuristic = tempNode.Value.GetStateForSite(actor, site).GetActivated(layer);
            }

            HeuristicLayerParameters parameters = layer.LayerParameters;


            #region Generating consequent
            double min = parameters.MinValue;
            double max = parameters.MaxValue;

            double consequentValue = activatedPriorPeriodHeuristic.Consequent.Value;

            switch (goalState.AnticipatedDirection)
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

            HeuristicConsequentPart consequent = HeuristicConsequentPart.Renew(activatedPriorPeriodHeuristic.Consequent, consequentValue);
            #endregion


            #region Generating antecedent
            List<HeuristicAntecedentPart> antecedentsList = new List<HeuristicAntecedentPart>(activatedPriorPeriodHeuristic.Antecedent.Length);
            
            foreach(HeuristicAntecedentPart antecedent in activatedPriorPeriodHeuristic.Antecedent)
            {
                dynamic newConst = actor[antecedent.Param];

                HeuristicAntecedentPart newAntecedent = HeuristicAntecedentPart.Renew(antecedent, newConst);

                antecedentsList.Add(newAntecedent);
            }
            #endregion

            Heuristic generatedHeuristic = Heuristic.Create(antecedentsList.ToArray(), consequent);

            layer.Add(generatedHeuristic);

            actor.AssignedHeuristics.Add(generatedHeuristic);


            //set consequent to actor's variables for next layers
            generatedHeuristic.Apply(actor);


            //Heuristic[] activatedPriorPeriodSiteHeuristics = priorPeriod.GetStateForSite(actor, site).Activated
            //    .Where(h=>h.Layer.Set == layer.Set)
            //    .OrderBy(h => h.Layer.PositionNumber)
            //    .ToArray();

            ////calculating new antecedent constant
            //double result = priorPeriod.TotalBiomass;

            //foreach (Heuristic heuristic in activatedPriorPeriodSiteHeuristics)
            //{
            //    if (heuristic == activatedPriorPeriodHeuristic)
            //    {
            //        break;
            //    }

            //    result = heuristic.ConsequentValue;
            //}



            //HeuristicParameters newHeuristicParams = new HeuristicParameters
            //{
            //    AntecedentConst = antecedentConst,
            //    AntecedentInequalitySign = activatedPriorPeriodHeuristic.AntecedentInequalitySign,
            //    ConsequentValue = consequentValue,
            //    IsAction = true,
            //    FreshnessStatus = 0
            //};


        }
    }
}