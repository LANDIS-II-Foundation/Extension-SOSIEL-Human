using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps
{
    using Actors;
    using Entities;
    using Enums;
    using Models;
    using Randoms;

    class InductiveReasoning
    {
        public void Execute(bool CTResult, Actor actor, LinkedListNode<PeriodModel> periodModel, Site site, HeuristicLayer layer)
        {
            //Inductive reasoning is activated only if heuristic-set-layer (i.e. CTResult) specific counterfactual thinking = unsuccessful (i.e. false).
            if (CTResult != false)
            {
                return;
            }

            PeriodModel currentPeriod = periodModel.Value;
            PeriodModel priorPeriod = periodModel.Previous.Value;

            Heuristic activatedPriorPeriodHeuristic = priorPeriod.GetDataForSite(actor, site).GetActivated(layer);

            HeuristicConsequentRule rule = layer.ConsequentRule;

            double min = rule.MinValue;
            double max = rule.MaxValue;

            double newConsequentValue = activatedPriorPeriodHeuristic.ConsequentValue;

            switch (currentPeriod.AnticipatedDirection)
            {
                case AnticipatedDirection.Up:
                    {
                        if (rule.ConsequentRelationship == ConsequentRelationship.Positive)
                        {
                            max = Math.Abs(activatedPriorPeriodHeuristic.ConsequentValue - max);

                            newConsequentValue += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }
                        if (rule.ConsequentRelationship == ConsequentRelationship.Negative)
                        {
                            max = Math.Abs(activatedPriorPeriodHeuristic.ConsequentValue - min);

                            newConsequentValue -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }

                        break;
                    }
                case AnticipatedDirection.Down:
                    {
                        if (rule.ConsequentRelationship == ConsequentRelationship.Positive)
                        {
                            max = Math.Abs(activatedPriorPeriodHeuristic.ConsequentValue - min);

                            newConsequentValue -= (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }
                        if (rule.ConsequentRelationship == ConsequentRelationship.Negative)
                        {
                            max = Math.Abs(activatedPriorPeriodHeuristic.ConsequentValue - max);

                            newConsequentValue += (Math.Abs(PowerLawRandom.GetInstance.Next(min, max) - max));
                        }

                        break;
                    }

                default:
                    {
                        throw new Exception("Not implemented for AnticipatedDirection == 'stay'");
                    }
            }

            Heuristic[] activatedPriorPeriodSiteHeuristic = priorPeriod.GetDataForSite(actor, site).
                Activated.OrderBy(h => h.Layer.PositionNumber).ToArray();

            //calculating new antecedent constant
            double result = priorPeriod.TotalBiomass;

            foreach (Heuristic heuristic in activatedPriorPeriodSiteHeuristic)
            {
                if (heuristic == activatedPriorPeriodHeuristic)
                {
                    break;
                }

                result = heuristic.ConsequentValue;
            }

            HeuristicParameters newHeuristicParams = new HeuristicParameters
            {
                AntecedentConst = result,
                AntecedentInequalitySign = activatedPriorPeriodHeuristic.AntecedentInequalitySign,
                AnticipatedInfluence = currentPeriod.AnticipatedInfluence,
                ConsequentValue = newConsequentValue,
                IsAction = activatedPriorPeriodHeuristic.IsAction,
                FreshnessStatus = 0
            };

            layer.Add(Heuristic.Create(newHeuristicParams));
        }
    }
}