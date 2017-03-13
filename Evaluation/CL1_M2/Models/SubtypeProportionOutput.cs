using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace CL1_M2.Models
{
    [DelimitedRecord(";")]
    class SubtypeProportionOutput
    {
        public int Iteration { get; set; }

        public double Proportion { get; set; }
    }
}
