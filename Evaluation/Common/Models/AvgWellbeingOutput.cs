using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    public class AvgWellbeingItem
    {
        public string Type { get; set; }

        public double AvgValue { get; set; }

        public override string ToString()
        {
            return $"{Type}: {AvgValue}";
        }
    }

    [DelimitedRecord(";")]
    public class AvgWellbeingOutput
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }

        //todo: replace on property
        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public AvgWellbeingItem[] Avgs; 
        
    }
}
