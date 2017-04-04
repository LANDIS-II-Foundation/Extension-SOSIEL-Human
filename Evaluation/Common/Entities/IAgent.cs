using System.Collections.Generic;

namespace Common.Entities
{
    using Configuration;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }
        int Id { get; set; }

        string PrototypeName { get; set; }

        List<Goal> Goals { get; set; }

        List<Rule> AssignedRules { get; set; }

        List<IAgent> ConnectedAgents { get; set; }

        void GenerateCustomParams();

        void SetToCommon(string key, dynamic value);

        void AssignRules(IEnumerable<string> assignedRules);
    }
}