using System.Linq;

namespace SocialHuman.Models
{
    using Actors;
    using Entities;

    public sealed class PeriodPartialModel
    {
        public Actor Actor { get; private set; }
        public Site Site { get; private set; }
        //public HeuristicSet HeuristicSet { get; private set; } 
        public Heuristic[] Matched { get; private set; }
        public Heuristic[] Activated { get; private set; }
        
        private PeriodPartialModel() { }

        public static PeriodPartialModel Create(Actor actor, Site site, Heuristic[] matched, Heuristic[] activated)
        {
            return new PeriodPartialModel
            {
                Actor = actor,
                Site = site,
                //HeuristicSet = set,
                Matched = matched,
                Activated = activated
            };
        }

        /// <summary>
        /// Returns activated heuristic for specific layer 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Heuristic GetActivated(HeuristicLayer layer)
        {
            return Activated.Single(h => h.Layer == layer);
        }
    }
}
