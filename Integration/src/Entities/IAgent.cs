using System.Collections.Generic;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Environments;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }

        string Id { get; }

        List<Goal> AssignedGoals { get; set; }

        List<Rule> AssignedRules { get; set; }

        List<IAgent> ConnectedAgents { get; set; }

        Dictionary<Rule, int> RuleActivationFreshness { get; set; }


        AgentPrototype Prototype { get; }

        /// <summary>
        /// Assign new rule to mental model (rule list) of current agent
        /// </summary>
        /// <param name="newRule"></param>
        void AssignNewRule(Rule newRule);

        /// <summary>
        /// Add rule to prototype rules and then assign one to the rule list of current agent
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        void AddRule(Rule newRule, RuleLayer layer);

        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetToCommon(string key, dynamic value);

        /// <summary>
        /// Check on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsVariable(string key);
    }
}