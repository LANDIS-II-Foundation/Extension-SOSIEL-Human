using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    public class RuleFrequenceItem
    {
        public string RuleId { get; set; }

        public int Frequence { get; set; }


        public override string ToString()
        {
            return $"{RuleId}: {Frequence}";
        }
    }

    [DelimitedRecord(";")]
    public class RuleFrequenciesOutput
    {
        [FieldOrder(0)]
        public int Iteration { get; set; }

        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public RuleFrequenceItem[] RuleFrequencies;
    }
}
