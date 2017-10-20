using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environments;
    using Exceptions;
    using Helpers;

    public class Agent : IAgent, ICloneable<Agent>, IEquatable<Agent>
    {
        protected int id;

        protected Dictionary<string, dynamic> privateVariables;

        public string Id { get { return Prototype.NamePrefix + id; } }

        public AgentPrototype Prototype { get; protected set; }

        public List<IAgent> ConnectedAgents { get; set; }

        public Dictionary<KnowledgeHeuristic, Dictionary<Goal, double>> AnticipationInfluence { get; protected set; }

        public List<KnowledgeHeuristic> AssignedKnowledgeHeuristics { get; protected set; }

        public List<Goal> AssignedGoals { get; protected set; }

        public Dictionary<Goal, GoalState> InitialGoalStates { get; protected set; }

        public Dictionary<KnowledgeHeuristic, int> KnowledgeHeuristicActivationFreshness { get; protected set; }

        public override string ToString()
        {
            return Id;
        }
        
        protected Agent()
        {
            privateVariables = new Dictionary<string, dynamic>();
            ConnectedAgents = new List<IAgent>();
            AnticipationInfluence = new Dictionary<KnowledgeHeuristic, Dictionary<Goal, double>>();
            InitialGoalStates = new Dictionary<Goal, GoalState>();
            AssignedKnowledgeHeuristics = new List<KnowledgeHeuristic>();
            AssignedGoals = new List<Goal>();
            KnowledgeHeuristicActivationFreshness = new Dictionary<KnowledgeHeuristic, int>();
        }


        public virtual dynamic this[string key]
        {
            get
            {
                if (privateVariables.ContainsKey(key))
                {
                    return privateVariables[key];
                }
                else
                {
                    if (Prototype.CommonVariables.ContainsKey(key))
                        return Prototype[key];
                }


                throw new UnknownVariableException(key);
            }
            set
            {
                if (privateVariables.ContainsKey(key) || Prototype.CommonVariables.ContainsKey(key))
                    PreSetValue(key, privateVariables[key]);

                if (Prototype.CommonVariables.ContainsKey(key))
                    Prototype[key] = value;
                else
                    privateVariables[key] = value;

                PostSetValue(key, value);
            }
        }


        /// <summary>
        /// Creates copy of current agent, after cloning need to set Id, connected agents don't copied
        /// </summary>
        /// <returns></returns>
        public virtual Agent Clone()
        {
            Agent agent = new Agent();

            agent.Prototype = Prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(privateVariables);

            agent.AssignedGoals = new List<Goal>(AssignedGoals);
            agent.AssignedKnowledgeHeuristics = new List<KnowledgeHeuristic>(AssignedKnowledgeHeuristics);

            //copy ai
            AnticipationInfluence.ForEach(kvp =>
            {
                agent.AnticipationInfluence.Add(kvp.Key, new Dictionary<Goal, double>(kvp.Value));
            });

            agent.KnowledgeHeuristicActivationFreshness = new Dictionary<KnowledgeHeuristic, int>(KnowledgeHeuristicActivationFreshness);

            return agent;
        }


        /// <summary>
        /// Checks on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsVariable(string key)
        {
            return privateVariables.ContainsKey(key) || Prototype.CommonVariables.ContainsKey(key);
        }


        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetToCommon(string key, dynamic value)
        {
            Prototype.CommonVariables[key] = value;
        }


        /// <summary>
        /// Handling variable after set to variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="newValue"></param>
        protected virtual void PostSetValue(string variable, dynamic newValue)
        {

        }


        /// <summary>
        /// Handling variable before set to variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="oldValue"></param>
        protected virtual void PreSetValue(string variable, dynamic oldValue)
        {

        }

        /// <summary>
        /// Assigns new heuristic to mental model (heuristic list) of current agent. If empty rooms ended, old heuristics will be removed.
        /// </summary>
        /// <param name="newHeuristic"></param>
        public void AssignNewHeuristic(KnowledgeHeuristic newHeuristic)
        {
            KnowledgeHeuristicsLayer layer = newHeuristic.Layer;

            KnowledgeHeuristic[] layerHeuristics = AssignedKnowledgeHeuristics.GroupBy(r => r.Layer).Where(g => g.Key == layer).SelectMany(g => g).ToArray();

            if (layerHeuristics.Length < layer.LayerConfiguration.MaxNumberOfHeuristics)
            {
                AssignedKnowledgeHeuristics.Add(newHeuristic);
                AnticipationInfluence.Add(newHeuristic, new Dictionary<Goal, double>());

                KnowledgeHeuristicActivationFreshness[newHeuristic] = 0;
            }
            else
            {
                KnowledgeHeuristic heuristicForRemoving = KnowledgeHeuristicActivationFreshness.Where(kvp => kvp.Key.Layer == layer && kvp.Key.IsAction).GroupBy(kvp => kvp.Value).OrderByDescending(g => g.Key)
                    .Take(1).SelectMany(g => g.Select(kvp => kvp.Key)).RandomizeOne();

                AssignedKnowledgeHeuristics.Remove(heuristicForRemoving);
                AnticipationInfluence.Remove(heuristicForRemoving);

                KnowledgeHeuristicActivationFreshness.Remove(heuristicForRemoving);

                AssignNewHeuristic(newHeuristic);
            }
        }

        /// <summary>
        /// Assigns new heuristic with defined anticipated influence to mental model (heuristic list) of current agent. If empty rooms ended, old heuristics will be removed. 
        /// Anticipated influence is copied to the agent.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="anticipatedInfluence"></param>
        public void AssignNewHeuristic(KnowledgeHeuristic newHeuristic, Dictionary<Goal, double> anticipatedInfluence)
        {
            AssignNewHeuristic(newHeuristic);

            //copy ai to personal ai for assigned goals only

            Dictionary<Goal, double> ai = anticipatedInfluence.Where(kvp => AssignedGoals.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            AnticipationInfluence[newHeuristic] = new Dictionary<Goal, double>(ai);
        }

        /// <summary>
        /// Adds heuristic to prototype heuristics and then assign one to the heuristic list of current agent.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="layer"></param>
        public void AddHeuristic(KnowledgeHeuristic newHeuristic, KnowledgeHeuristicsLayer layer)
        {
            Prototype.AddNewHeuristic(newHeuristic, layer);

            AssignNewHeuristic(newHeuristic);
        }


        /// <summary>
        /// Adds heuristic to prototype heuristics and then assign one to the heuristic list of current agent. 
        /// Also copies anticipated influence to the agent.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="layer"></param>
        /// <param name="anticipatedInfluence"></param>
        public void AddHeuristic(KnowledgeHeuristic newHeuristic, KnowledgeHeuristicsLayer layer, Dictionary<Goal, double> anticipatedInfluence)
        {
            Prototype.AddNewHeuristic(newHeuristic, layer);

            AssignNewHeuristic(newHeuristic, anticipatedInfluence);
        }

        /// <summary>
        /// Sets id to current agent instance.
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            this.id = id;
        }



        /// <summary>
        /// Equality checking.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Agent other)
        {
            return ReferenceEquals(this, other)
                || (other != null && Id == other.Id);
        }


        public override bool Equals(object obj)
        {
            return base.Equals(obj) || Equals(obj as Agent);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Agent a, Agent b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Agent a, Agent b)
        {
            return !(a == b);
        }


    }
}
