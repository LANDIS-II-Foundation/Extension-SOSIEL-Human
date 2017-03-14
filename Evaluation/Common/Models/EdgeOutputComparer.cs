using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Models
{
    public class EdgeOutputComparer : IEqualityComparer<EdgeOutput>
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
