using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public interface IConfigurableAgent: IAgent
    {
        List<Rule> AssignedRules { get; set; }

        List<Goal> Goals { get; set; }

        void SyncState(IEnumerable<string> assignedRules);
    }
}
