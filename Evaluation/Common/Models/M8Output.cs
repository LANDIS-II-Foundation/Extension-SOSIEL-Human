using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{

    [DelimitedRecord(";")]
    public class M8Output
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }

        [FieldOrder(1)]
        public double PoolWellbeing { get; set; }

        [FieldOrder(2)]
        public double[] AgentWellbeings { get; set; }
    }
}
