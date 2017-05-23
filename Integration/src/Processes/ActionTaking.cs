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

            state.TakenActions.Add(site, new List<TakenAction>());

            history.Activated.OrderBy(r => r.Layer.Set).ThenBy(r => r.Layer).ForEach(r =>
               {
                   TakenAction result = r.Apply(agent);

                   //add result to the state
                   state.TakenActions[site].Add(result);
               });
        }
    }
}
