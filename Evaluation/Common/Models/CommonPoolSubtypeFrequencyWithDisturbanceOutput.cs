using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class CommonPoolSubtypeFrequencyWithDisturbanceOutput
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }

        [FieldOrder(1)]
        public double Disturbance { get; set; }

        [FieldOrder(2)]
        [FieldConverter(typeof(ToStringConverter))]
        public int[] IntervalFrequency;

        public static explicit operator CommonPoolSubtypeFrequencyWithDisturbanceOutput(CommonPoolSubtypeFrequencyOutput record)
        {
            return new CommonPoolSubtypeFrequencyWithDisturbanceOutput { Iteration = record.Iteration, IntervalFrequency = record.IntervalFrequency };
        }
    }
}
