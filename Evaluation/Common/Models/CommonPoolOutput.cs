using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    public class CommonPoolCenter
    {
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return $"x: {X}| y: {Y}";
        }

    }

    public class CommonPoolProportion
    {
        public CommonPoolCenter Center { get; set; }

        public double Wellbeing { get; set; }

        public double CoProportion { get; set; }
    }

    public class CommonPoolOutput
    {
        public int Iteration { get; set; } 

        public CommonPoolProportion[] CommonPools { get; set; }

        public override string ToString()
        {
            string line1 = string.Join(";", CommonPools.Select(cp => cp.Center.ToString()));
            string line2 = string.Join(";", CommonPools.Select(cp => cp.Wellbeing));
            string line3 = string.Join(";", CommonPools.Select(cp => cp.CoProportion));

            return $"{Iteration};{line1}{Environment.NewLine};{line2}{Environment.NewLine};{line3}";
        }
    }


}
