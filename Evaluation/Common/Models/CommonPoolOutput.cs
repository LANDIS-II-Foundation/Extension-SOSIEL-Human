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

    public class CommonPoolStat
    {
        public CommonPoolCenter Center { get; set; }

        public double CommonPoolWellbeing { get; set; }

        //public double CoProportion { get; set; }

        //public double NonCoProportion { get; set; }
    }

    public class CommonPoolOutput
    {
        public int Iteration { get; set; } 

        public CommonPoolStat[] CommonPools { get; set; }

        public override string ToString()
        {
            string line1 = $"{nameof(CommonPoolStat.Center)};{string.Join(";", CommonPools.Select(cp => cp.Center.ToString()))}";
            string line2 = $"{nameof(CommonPoolStat.CommonPoolWellbeing)};{string.Join(";", CommonPools.Select(cp => cp.CommonPoolWellbeing))}";
            //string line3 = $"{nameof(CommonPoolProportion.CoProportion)};{string.Join(";", CommonPools.Select(cp => cp.CoProportion))}";

            //string line4 = string.Empty;

            //if (CommonPools.Any(cp => cp.NonCoProportion != 0))
            //    line4= $"{Environment.NewLine};{nameof(CommonPoolProportion.NonCoProportion)};{string.Join(";", CommonPools.Select(cp => cp.NonCoProportion))}";

            return $"{Iteration};{line1}{Environment.NewLine};{line2}{Environment.NewLine}";
        }
    }


}
