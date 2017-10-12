using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace Common.Entities
{
    using Exceptions;
    using Helpers;


    public class AgentPrototype
    {
        public string NamePrefix { get; private set; }

        public Dictionary<string, dynamic> CommonVariables { get; private set; }

        public List<Goal> Goals { get; private set; }

        public Dictionary<string, MentalModelConfiguration> MentalModel { get; private set; }

        [JsonProperty]
        public List<KnowledgeHeuristic> KnowledgeHeuristics { get; }


        public Dictionary<string, double> DoNothingAnticipatedInfluence { get; private set; }


        private List<MentalModel> mentalProto;

        public List<MentalModel> MentalProto
        {
            get { return mentalProto == null ? TransformKhToMentalModel() : mentalProto; }
        }

        public bool IsSiteOriented { get; set; }

        public bool UseImportanceAdjusting { get; set; }

        public AgentPrototype()
        {
            CommonVariables = new Dictionary<string, dynamic>();
            MentalModel = new Dictionary<string, MentalModelConfiguration>();
            KnowledgeHeuristics = new List<KnowledgeHeuristic>();
        }

        public dynamic this[string key]
        {
            get
            {
                if (CommonVariables.ContainsKey(key))
                    return CommonVariables[key];

                throw new UnknownVariableException(key);
            }
            set
            {
                CommonVariables[key] = value;
            }

        }
        
        /// <summary>
        /// Transforms from kh list to mental model
        /// </summary>
        /// <returns></returns>
        private List<MentalModel> TransformKhToMentalModel()
        {
            mentalProto = KnowledgeHeuristics.GroupBy(kh => kh.MentalModel).OrderBy(g => g.Key).Select(g =>
                   new MentalModel(g.Key, Goals.Where(goal => MentalModel[g.Key.ToString()].AssociatedWith.Contains(goal.Name)).ToArray(),
                       g.GroupBy(kh => kh.KnowledgeHeuristicsLayer).OrderBy(g2 => g2.Key).
                       Select(g2 => new KnowledgeHeuristicsLayer(MentalModel[g.Key.ToString()].Layer[g2.Key.ToString()], g2)))).ToList();

            return mentalProto;
        }



        /// <summary>
        /// Adds heuristic to mental model of current prototype if it isn't exists in the scope.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="layer"></param>
        public void AddNewHeuristic(KnowledgeHeuristic newHeuristic, KnowledgeHeuristicsLayer layer)
        {
            if (mentalProto == null)
                TransformKhToMentalModel();

            layer.Add(newHeuristic);

            KnowledgeHeuristics.Add(newHeuristic);
        }


        /// <summary>
        /// Checks for similar heuristics
        /// </summary>
        /// <param name="heuristic"></param>
        /// <returns></returns>
        public bool IsSimilarHeuristicExists(KnowledgeHeuristic heuristic)
        {
            return KnowledgeHeuristics.Any(kh => kh == heuristic);
        }


        /// <summary>
        /// Adds do nothing heuristic to each heuristic set and heuristic layer
        /// </summary>
        /// <returns>Returns created heuristic ids</returns>
        public IEnumerable<string> AddDoNothingHeuristics()
        {
            List<string> temp = new List<string>();

            MentalProto.ForEach(mm =>
            {
                //only for layers with UseDoNothing: true configuration
                mm.Layers.Where(layer => layer.LayerConfiguration.UseDoNothing).ForEach(layer =>
                {
                    if (!layer.KnowledgeHeuristics.Any(kh => kh.IsAction == false))
                    {
                        KnowledgeHeuristic proto = layer.KnowledgeHeuristics.First();

                        KnowledgeHeuristic doNothing = KnowledgeHeuristic.Create(
                            new KnowledgeHeuristicAntecedentPart[] { new KnowledgeHeuristicAntecedentPart(SosielVariables.AgentStatus, "==", true) },
                            KnowledgeHeuristicConsequent.Renew(proto.Consequent, Activator.CreateInstance(proto.Consequent.Value.GetType())),
                            false, false, 1, true
                        );

                        AddNewHeuristic(doNothing, layer);

                        temp.Add(doNothing.Id);
                    }
                });
            });

            return temp;
        }

    }
}
