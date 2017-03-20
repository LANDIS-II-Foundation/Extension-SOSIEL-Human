using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    public class AvgVariableItem
    {
        public string Name { get; set; }

        public double Value { get; set; }


        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }

    [DelimitedRecord(";")]
    public class AvgVariablesOutput
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }


        //todo: replace on property
        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public AvgVariableItem[] Avgs;
    }
}
