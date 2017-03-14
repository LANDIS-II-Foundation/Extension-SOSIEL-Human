using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class SubtypeProportionOutput
    {
        public int Iteration { get; set; }

        public double Proportion { get; set; }
    }
}
