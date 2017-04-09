using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    public class NumericValuesItem
    {
        public string Name { get; set; }

        public bool IsFirstVariable { get; set; }

        public dynamic[] Values { get; set; }


        public override string ToString()
        {
            return $"{(IsFirstVariable ? "" : ";")}{Name};{string.Join(";", Values.Select(v => v.ToString("0.000")))}{Environment.NewLine}";
        }
    }

    [DelimitedRecord(";")]
    public class AgentNumericValuesOutput : IHeader
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }

        public string HeaderLine
        {
            get
            {
                NumericValuesItem first = Values.FirstOrDefault();

                return $"Iteration;Variable;{(first?.Values != null ? string.Join(";", Enumerable.Range(1, first.Values.Length).Select(n => $"Agent{n}")) : "")}";
            }
        }


        //todo: replace on property
        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public NumericValuesItem[] Values;
    }
}
