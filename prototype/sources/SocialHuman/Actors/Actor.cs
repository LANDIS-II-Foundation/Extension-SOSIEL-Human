using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Actors
{
    using Enums;
    using Entities;
    using Models;

    public abstract class Actor
    {
        protected string ClassName { get; private set; }

        #region Public fields
        public string ActorName { get; private set; }
        public ActorType ActorType { get; private set; }

        public ActorGoal[] Goals { get; private set; }
        public List<HeuristicSet> MentalModel { get; protected set; }
        public List<Site> AssignedSites { get; private set; }
        public HeuristicConsequentRule[] HeuristicsConsequentRules { get; private set; }
        #endregion


        #region Abstract methods
        public abstract void SimulatePart1(LinkedListNode<Period> periodModel);
        public abstract void SimulatePart2(LinkedListNode<Period> periodModel);
        public abstract void SimulateTakeActionPart(LinkedListNode<Period> periodModel);
        #endregion

        #region Constructors
        protected Actor(ActorParameters parameters, Site[] sites)
        {
            ActorName = parameters.ActorName;
            ClassName = parameters.ClassName;
            ActorType = parameters.ActorType;
            MentalModel = new List<HeuristicSet>();
            Goals = parameters.Goals;
            HeuristicsConsequentRules = parameters.HeuristicConsequentRules;

            foreach (var set in parameters.Heuristics.GroupBy(h => h.Set).OrderBy(s => s.Key))
            {
                HeuristicSet newSet = new HeuristicSet(set.Key);
                MentalModel.Add(newSet);

                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(l => l.Key))
                {
                    HeuristicConsequentRule rule = HeuristicsConsequentRules.Single(cr => cr.HeuristicLayer == layer.Key);

                    HeuristicLayer newLayer = new HeuristicLayer(Global.Instance.MaxHeuristicInLayer, rule);
                    newSet.Add(newLayer);

                    foreach (var heuristic in layer.OrderBy(h => h.PositionNumber))
                    {
                        newLayer.Add(Heuristic.Create(heuristic, Goals));
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
        #endregion

        #region Public methods
        public bool IsSiteAssigned(Site site)
        {
            return AssignedSites.Any(s => s.Equals(site));
        }
        #endregion

    }
}
