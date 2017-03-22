using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Entities
{
    public class IterationState
    {
        public int Iteration { get; set; }

        public Dictionary<IAgent, AgentState> AgentsState { get; set; }






    }
}
