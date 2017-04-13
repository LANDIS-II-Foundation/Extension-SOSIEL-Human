using System.Collections.Generic;

namespace Common.Entities
{
    using Configuration;
    using Enums;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }
        int Id { get; set; }

        string PrototypeName { get; set; }

        List<Goal> Goals { get; set; }

        List<Rule> AssignedRules { get; set; }

        List<IAgent> ConnectedAgents { get; set; }

        IEnumerable<Rule> MentalModelRules { get; }

        AgentStateConfiguration InitialStateConfiguration { get; set; }

        SocialNetworkType SocialNetwork { get; set; }

        Dictionary<Rule, int> RuleActivationFreshness { get; set; }

        void GenerateCustomParams();

        void SetToCommon(string key, dynamic value);

        void AssignInitialRules(IEnumerable<string> assignedRules);

        void AssignNewRule(Rule newRule);

        bool ContainsVariable(string key);
    }
}