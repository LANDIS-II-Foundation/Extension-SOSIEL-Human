using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class SubtypeProportionOutput: IHeader
    {
        public string HeaderLine
        {
            get
            {
                return $"Iteration;{Subtype}_Proportion";
            }
        }

        [FieldHidden]
        public string Subtype;

        [FieldOrder(0)]
        public int Iteration { get; set; }

        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public double Proportion;
    }
}
