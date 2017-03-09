using System;
using System.Collections.Generic;
using System.Linq;

using FileHelpers;

namespace CL1_M1.Models
{
    [DelimitedRecord(";")]
    public class NodeOutput
    {
        public int AgentId { get; set; }

        public AgentType Type { get; set; }
    }
}
