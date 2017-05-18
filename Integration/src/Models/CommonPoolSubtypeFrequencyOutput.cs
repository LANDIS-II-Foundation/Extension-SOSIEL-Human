using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{
    [DelimitedRecord(";")]
    public class Landis.Extension.SosielPoolSubtypeFrequencyOutput:IHeader
    {
        public string HeaderLine
        {
            get
            {
                return "Iteration;0.1;0.2;0.3;0.4;0.5;0.6;0.7;0.8;0.9;1";
            }
        }

        [FieldOrder(0)]
        public int Iteration { get; set; }

        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public int[] IntervalFrequency;
    }
}
