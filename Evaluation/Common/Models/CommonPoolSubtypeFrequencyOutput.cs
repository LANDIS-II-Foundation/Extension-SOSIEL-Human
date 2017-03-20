using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class CommonPoolSubtypeFrequencyOutput
    {
        public int Iteration { get; set; }

        public double Disturbance { get; set; }

        [FieldConverter(typeof(ToStringConverter))]
        public int[] IntervalFrequency;
    }
}
