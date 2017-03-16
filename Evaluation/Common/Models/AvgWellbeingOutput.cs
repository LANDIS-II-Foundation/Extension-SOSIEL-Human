using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Models
{
    public class AvgWellbeing
    {
        public string Type { get; set; }

        public double AvgValue { get; set; }

        public override string ToString()
        {
            return $"{Type}: {AvgValue}";
        }
    }

    public class AvgWellbeingOutput
    {
        public int Iteration { get; set; }

        public AvgWellbeing[] Avgs { get; set; }

        public override string ToString()
        {
            return $"{Iteration};{string.Join(";", Avgs.Select(aw=>aw.ToString()))}";
        }
    }
}
