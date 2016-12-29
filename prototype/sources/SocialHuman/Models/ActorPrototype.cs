using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace SocialHuman.Models
{
    using Input = Parsers.Models;

    public sealed class ActorPrototype
    {
        #region Private fields
        #endregion

        #region Public fields
        public int Type { get; private set; }

        public Dictionary<string, dynamic> Variables { get; private set; } = new Dictionary<string, dynamic>();

        public Goal[] Goals { get; private set; }

        public HeuristicSet[] MentalModel { get; private set; }

        public IEnumerable<Heuristic> HeuristicEnumerable
        {
            get
            {
                return MentalModel.SelectMany(s => s.Layers.SelectMany(l => l.Heuristics));
            }
        }

        public dynamic this[string key]
        {
            get
            {
                if (Variables.ContainsKey(key))
                    return Variables[key];
                else
                {
                    return null;
                }
            }
            set
            {
                Variables[key] = value;
            }

        }
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public methods

        #endregion

        #region Factory methods
        internal static ActorPrototype Create(Input.ActorPrototype input)
        {
            ActorPrototype prototype = Mapper.Map<ActorPrototype>(input);

            prototype.MentalModel = CreateHeuristicSets(prototype.Goals, input.Heuristics, input.HeuristicLayerParameters, input.HeuristicSetParameters).ToArray();


            return prototype;
        }

        internal static IEnumerable<HeuristicSet> CreateHeuristicSets(Goal[] goals, Input.Heuristic[] heuristics, Input.HeuristicLayerParameters[] layerParameters,
            Input.HeuristicSetParameters[] setParameters)
        {
            foreach (var _set in heuristics.GroupBy(h => h.Set))
            {
                string[] associatedWith = setParameters.Single(sp => sp.HeuristicSet == _set.Key).AssociatedWith;

                HeuristicSet set = new HeuristicSet(_set.Key, goals.Where(g => associatedWith.Contains(g.Name)).ToArray());

                foreach (var _layer in _set.GroupBy(s => s.Layer).OrderBy(g => g.Key))
                {
                    Input.HeuristicLayerParameters parameters = layerParameters.Single(lp => lp.HeuristicSet == _set.Key && lp.HeuristicLayer == _layer.Key);

                    HeuristicLayer layer = new HeuristicLayer(Mapper.Map<HeuristicLayerParameters>(parameters));

                    layer.AddRange(Mapper.Map<List<Heuristic>>(_layer.AsEnumerable()));

                    set.Add(layer);
                }

                yield return set;
            }
        }
        #endregion region

    }
}
