using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    using Configuration;

    public interface IConfigurableAgent: IAgent
    {
        List<Rule> AssignedRules { get; set; }

        AgentStateConfiguration InitialState { get; set; }

        GoalsSettings GoalsSettings { get; set; }

        void SetToCommon(string key, dynamic value);

        void AssignRules(IEnumerable<string> assignedRules);
    }
}
