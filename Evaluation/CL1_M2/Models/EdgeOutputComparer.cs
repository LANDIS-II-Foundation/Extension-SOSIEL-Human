using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace CL1_M2.Models
{
    class EdgeOutputComparer : IEqualityComparer<EdgeOutput>
    {
        public bool Equals(EdgeOutput x, EdgeOutput y)
        {
            return x.AgentId == y.AdjacentAgentId && x.AdjacentAgentId == y.AgentId;
        }

        public int GetHashCode(EdgeOutput obj)
        {
            return 0;
        }
    }
}
