using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    public class ValueItem
    {
        public string Name { get; set; }

        public double Value { get; set; }


        public override string ToString()
        {
            return $"{Value.ToString("0.000")}";
        }
    }

    [DelimitedRecord(";")]
    public class CommonValuesOutput: IHeader
    {
        public string HeaderLine
        {
            get
            {
                return $"Iteration;{(Values != null ? string.Join(";", Values.Select(v=>v.Name)) : "")}";
            }
        }


        [FieldOrder(0)]
        public int Iteration { get; set; }

        //todo: replace on property
        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public ValueItem[] Values;
    }
}
