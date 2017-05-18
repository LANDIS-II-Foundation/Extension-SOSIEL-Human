using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Configuration;
    using Environments;
    using Exceptions;
    using Helpers;

    public class Agent: IAgent, ICloneable<Agent>, IEquatable<Agent>
    {
        private int id;
        private AgentPrototype prototype;

        private Dictionary<string, dynamic> privateVariables { get; set; } 


        public string Id { get { return prototype.NamePrefix + id; } }

        public AgentPrototype Prototype { get { return prototype; } }

        public List<IAgent> ConnectedAgents { get; set; }

        public List<Rule> AssignedRules { get; set; } = new List<Rule>();

        public List<Goal> AssignedGoals { get; set; }

        public Dictionary<Rule, int> RuleActivationFreshness { get; set; }


        public override string ToString()
        {
            return Id;
        }

       

        /// <summary>
        /// Closed agent constructor
        /// </summary>
        private Agent() {
            RuleActivationFreshness = new Dictionary<Rule, int>();
        }

        
        public virtual dynamic this[string key]
        {
            get
            {
                if (privateVariables.ContainsKey(key))
                    return privateVariables[key];

                throw new UnknownVariableException(key);
            }
            set
            {
                if (privateVariables.ContainsKey(key) || prototype.CommonVariables.ContainsKey(key))
                    PreSetValue(key, privateVariables[key]);

                if (prototype.CommonVariables.ContainsKey(key))
                    prototype.CommonVariables[key] = value;
                else 
                    privateVariables[key] = value;

                PostSetValue(key, value);
            }

        }


        /// <summary>
        /// Create copy of current agent, after cloning need to set Id, connected agents don't copied
        /// </summary>
        /// <returns></returns>
        public Agent Clone()
        {
            Agent agent = new Agent();

            agent.prototype = prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(privateVariables);

            agent.AssignedGoals = new List<Goal>(AssignedGoals);
            agent.AssignedRules = new List<Rule>(AssignedRules);
            
            agent.RuleActivationFreshness = new Dictionary<Rule, int>(RuleActivationFreshness);

            return agent;
        }


        /// <summary>
        /// Check on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsVariable(string key)
        {
            return privateVariables.ContainsKey(key) || prototype.CommonVariables.ContainsKey(key);
        }


        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetToCommon(string key, dynamic value)
        {
            prototype.CommonVariables[key] = value;
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
        /// Assign new rule to mental model (rule list) of current agent
        /// </summary>
        /// <param name="newRule"></param>
        public void AssignNewRule(Rule newRule)
        {
            RuleLayer layer = newRule.Layer;

            Rule[] layerRules = AssignedRules.GroupBy(r => r.Layer).Where(g => g.Key == layer).SelectMany(g => g).ToArray();

            if (layerRules.Length < layer.LayerSettings.MaxNumberOfRules)
            {
                AssignedRules.Add(newRule);

                RuleActivationFreshness[newRule] = 0;
            }
            else
            {
                Rule ruleForRemoving = RuleActivationFreshness.Where(kvp => kvp.Key.Layer == layer && kvp.Key.IsAction).GroupBy(kvp => kvp.Value).OrderByDescending(g => g.Key)
                    .Take(1).SelectMany(g => g.Select(kvp => kvp.Key)).RandomizeOne();

                AssignedRules.Remove(ruleForRemoving);

                RuleActivationFreshness.Remove(ruleForRemoving);

                AssignNewRule(newRule);
            }
        }

        /// <summary>
        /// Add rule to prototype rules and then assign one to the rule list of current agent
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        public void AddRule(Rule newRule, RuleLayer layer)
        {
            prototype.AddNewRule(newRule, layer);

            AssignNewRule(newRule);
        }


        /// <summary>
        /// Create agent instance based on agent prototype and agent configuration 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="agentConfiguration"></param>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public static Agent CreateAgent(AgentStateConfiguration agentConfiguration, AgentPrototype prototype)
        {
            Agent agent = new Agent();

            agent.prototype = prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(agentConfiguration.PrivateVariables);

            agent.AssignedRules = prototype.Rules.Where(r => agentConfiguration.AssignedRules.Contains(r.Id)).ToList();
            agent.AssignedGoals = prototype.Goals.Where(g => agentConfiguration.AssignedGoals.Contains(g.Name)).ToList();

            return agent;
        }


        /// <summary>
        /// Set id to current agent instance 
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            this.id = id;
        }



        /// <summary>
        /// Equality checking
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Agent other)
        {
            return other!= null && Id == other.Id;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as Agent);
        }

        
    }
}
