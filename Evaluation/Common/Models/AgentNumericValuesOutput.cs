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

        public dynamic[] Values { get; set; }


        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, string.Join(";", Values.Select(v => v.ToString("0.000"))));
        }
    }

    [DelimitedRecord(";")]
    public class AgentNumericValuesOutput
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }


        //todo: replace on property
        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public List<NumericValuesItem> Values;
    }
}
