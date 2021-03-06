﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{

    [DelimitedRecord(";")]
    public class AgentContributionsOutput
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }

        [FieldOrder(1)]
        public double[] AgentContributions { get; set; }
    }
}
