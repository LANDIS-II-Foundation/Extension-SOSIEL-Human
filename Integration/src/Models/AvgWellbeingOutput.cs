using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{
    public class AvgWellbeingItem
    {
        public string Type { get; set; }

        public double AvgValue { get; set; }

        public override string ToString()
        {
            return string.Format("{0:0.000}", AvgValue);
        }
    }

    [DelimitedRecord(";")]
    public class AvgWellbeingOutput: IHeader
    {
        public string HeaderLine
        {
            get
            {
                return "Iteration;Co;NonCo";
            }
        }

        [FieldOrder(0)]
        public int Iteration { get; set; }



        //todo: replace on property
        [FieldOrder(1)]
        [FieldConverter(typeof(ToStringConverter))]
        public AvgWellbeingItem[] Avgs; 
        
    }
}
