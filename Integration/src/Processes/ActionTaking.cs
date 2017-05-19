using System.Collections.Generic;
using System.Linq;

using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Enums;
    using Entities;
    using Helpers;

    public class ActionTaking
    {
        public void Execute(IAgent agent, AgentState state, ActiveSite site)
        {
            RuleHistory history = state.RuleHistories[site];

            history.Activated.OrderBy(r => r.Layer.Set).ThenBy(r => r.Layer).ForEach(r =>
               {
                   r.Apply(agent);
               });
        }
    }
}
