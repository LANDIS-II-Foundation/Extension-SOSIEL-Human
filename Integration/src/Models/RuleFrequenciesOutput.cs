using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{
    public class RuleFrequenceItem
    {
        public string RuleId { get; set; }

        public int Frequence { get; set; }


        public override string ToString()
        {
            return Frequence.ToString();
        }
    }

    [DelimitedRecord(";")]
    public class RuleFrequenciesOutput:IHeader
    {
        public string HeaderLine
        {
            get
            {
                return $"Iteration;{(RuleFrequencies != null ? string.Join(";", RuleFrequencies.Select(i=>i.RuleId)) : "")}";

            }
        }

        [FieldOrder(0)]
        public int Iteration { get; set; }

        

        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public RuleFrequenceItem[] RuleFrequencies;
    }
}
