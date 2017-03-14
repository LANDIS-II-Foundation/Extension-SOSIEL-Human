using System;
using System.Collections.Generic;
using System.Linq;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class EdgeOutput 
    {
        public int AgentId { get; set; }

        public int AdjacentAgentId { get; set; }

      
    }
}
