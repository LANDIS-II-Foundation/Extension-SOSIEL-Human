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

        void SetToCommon(string key, dynamic value);

        void SyncState(IEnumerable<string> assignedRules);
    }
}
