using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Enums;
    using Entities;
    using Helpers;

    public class ActionTaking
    {
        public void Execute(IAgent agent, AgentState state)
        {
            //Sequential set apply earlier than simultaneous. Ones apply after AS process
            state.Activated.Where(r=>r.Layer.Set.IsSequential == false).OrderBy(r => r.Layer.Set).ThenBy(r => r.Layer).ForEach(r =>
               {
                   r.Apply(agent);
               });
        }

        public void ExecuteForSpecificRuleSet(IAgent agent, AgentState state, RuleSet set)
        {
            //single actions only 
            state.Activated.Where(r => r.Layer.Set == set && r.IsCollectiveAction == false).OrderBy(r => r.Layer).ForEach(r =>
              {
                  r.Apply(agent);
              });
        }

    }
}
