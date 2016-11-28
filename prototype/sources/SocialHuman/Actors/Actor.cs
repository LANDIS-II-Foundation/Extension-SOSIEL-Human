using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Actors
{
    using Enums;
    using Entities;
    using Models;

    public abstract class Actor
    {
        protected string name;
        protected string ClassName { get; private set; }
        public string Name { get { return name; } }
        public ActorType ActorType { get; private set; }
        public List<HeuristicSet> MentalModel { get; protected set; }
        public List<Site> AssignedSites { get; private set; }
        public HeuristicConsequentRule[] HeuristicsConsequentRules { get; private set; }


        public abstract void SimulatePart1(LinkedListNode<PeriodModel> periodModel);
        public abstract void SimulatePart2(LinkedListNode<PeriodModel> periodModel);
        public abstract void SimulateTakeActionPart(LinkedListNode<PeriodModel> periodModel);

        protected Actor(ActorParameters parameters, Site[] sites)
        {
            ClassName = parameters.ClassName;
            ActorType = parameters.ActorType;
            MentalModel = new List<HeuristicSet>();
            HeuristicsConsequentRules = parameters.HeuristicsConsequentRules;

            foreach (var set in parameters.Heuristics.GroupBy(h => h.Set).OrderBy(s=>s.Key))
            {
                HeuristicSet newSet = new HeuristicSet(set.Key);
                MentalModel.Add(newSet);

                foreach(var layer in set.GroupBy(h=>h.Layer).OrderBy(l=>l.Key))
                {
                    HeuristicConsequentRule rule = HeuristicsConsequentRules.Single(cr => cr.HeuristicLayer == layer.Key);

                    HeuristicLayer newLayer = new HeuristicLayer(Global.Instance.MaxHeuristicInLayer, rule);
                    newSet.Add(newLayer);

                    foreach(var heuristic in layer.OrderBy(h => h.PositionNumber))
                    {
                        newLayer.Add(Heuristic.Create(heuristic));
                    }
                }
            }

            AssignedSites = new List<Site>();

            for (int i = 0; i < sites.Length; i++)
            {
                if (parameters.AssignedSites[i])
                {
                    AssignedSites.Add(sites[i]);
                }
            }

        }

        public bool IsSiteAssigned(Site site)
        {
            return AssignedSites.Any(s => s.Equals(site));
        }
    }
}
