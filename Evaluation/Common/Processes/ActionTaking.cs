using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;
    using Helpers;

    public class ActionTaking
    {
        public void Execute(IAgent agent, AgentState state)
        {
            state.Activated.OrderBy(r => r.Layer.Set).ThenBy(r => r.Layer).ForEach(r =>
               {
                   r.Apply(agent);
               });
        }
    }
}
