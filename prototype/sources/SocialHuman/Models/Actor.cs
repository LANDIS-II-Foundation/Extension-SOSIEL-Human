using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SocialHuman.Models
{
    using Enums;
    
    using Input = Parsers.Models;

    public sealed class Actor
    {
        #region Private fields

        #endregion

        #region Public fields
        public ActorPrototype Prototype { get; private set; }

        public string ActorName { get; private set; }

        public Dictionary<string, dynamic> Variables { get; private set; }

        public GoalState[] AssignedGoals { get; private set; }

        public List<Heuristic> AssagnedHeuristics { get; private set; }

        public List<Heuristic> BlockedHeuristics { get; private set; } = new List<Heuristic>();

        public List<AnticipatedInfluence> AnticipatedInfluences { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public dynamic this[string key]
        {
            get
            {
                if (Variables.ContainsKey(key))
                    return Variables[key];
                else
                {
                    if (Prototype.Variables.ContainsKey(key))
                        return Prototype.Variables[key];
                    else
                        return null;
                }
            }
            set
            {
                if (Prototype.Variables.ContainsKey(key))
                    Prototype.Variables[key] = value;
                else
                {
                    Variables[key] = value;
                }
            }
            
        }

        public bool IsSiteSpecific
        {
            get
            {
                return this[VariablesName.IsSiteSpecific];
            }
        }
        #endregion

        #region Constructors
        private Actor(string actorName, ActorPrototype prototype)
        {
            ActorName = actorName;
            Prototype = prototype;
        }
        #endregion

        #region Public methods
        //public bool IsSiteAssigned(Site site)
        //{
        //    return AssignedSites.Any(s => s.Equals(site));
        //}

        public bool ContainsVariable(string name)
        {
            return Variables.ContainsKey(name) || Prototype.Variables.ContainsKey(name);
        }
        #endregion

        #region Factory method
        internal static Actor Create(string actorName, ActorPrototype prototype, Input.Actor input)
        {
            Actor actor = new Actor(actorName, prototype);

            actor.Variables = input.Variables;

            actor.AssagnedHeuristics = actor.Prototype.HeuristicEnumerable.Where(h => input.AssagnedHeuristics.Contains(h.Id)).ToList();

            actor.AssignedGoals = input.AssignedGoals.Join(prototype.Goals, gs => gs.Name, g => g.Name, (gs, g) => new GoalState((Goal)g.Clone(), gs.Value))
                .ToArray();

            actor.AnticipatedInfluences = actor.AssagnedHeuristics
                .SelectMany(h => actor.AssignedGoals.Select(g => 
                    new AnticipatedInfluence(h, g.Goal, input.AnticipatedInfluences.ContainsKey(h.Id) ? input.AnticipatedInfluences[h.Id] : null))).ToList();

            
            if (input.Variables.ContainsKey(VariablesName.AssignedSites))
            {
                bool[] assignedSiteFlags = ((IEnumerable<JToken>)input.Variables[VariablesName.AssignedSites]).Select(v => v.ToObject<bool>()).ToArray();

                actor[VariablesName.AssignedSites] = assignedSiteFlags;

                actor[VariablesName.IsSiteSpecific] = true;
            }
            else
            {
                actor[VariablesName.IsSiteSpecific] = false;
            }

            if(input.Variables.ContainsKey(VariablesName.SocialNetworks))
            {
                string[] networks = ((IEnumerable<JToken>)input.Variables[VariablesName.SocialNetworks]).Select(v => v.ToObject<string>()).ToArray();

                actor[VariablesName.SocialNetworks] = networks;
            }



            //if (input.Variables.ContainsKey(VariablesName.Wealth))
            //{
            //    double[] harvested = ((IEnumerable<JToken>)input.Variables[VariablesName.Wealth]).Select(v => v.ToObject<double>()).ToArray();

            //    actor[VariablesName.Wealth] = harvested;
            //}
            
            return actor;
        }
        #endregion
    }
}
