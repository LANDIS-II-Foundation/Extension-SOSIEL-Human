using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Randoms
{
    public class NormalDistributionRandom
    {
        private static NormalDistributionRandom random;

        double _mean;
        double _stndDeviation;

        public double Next()
        {
            return Next(_mean, _stndDeviation);
        }


        public double Next(double mean, double stndDeviation)
        {
            Random r = LinearUniformRandom.GetInstance;

            double u1 = 1 - r.NextDouble();
            double u2 = 1 - r.NextDouble();

            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);


            return mean + stndDeviation * randStdNormal;
        }

        public static NormalDistributionRandom GetInstance
        {
            get
            {
                if (random == null)
                    random = new NormalDistributionRandom(0.3,0.3);

                return random;
            }
        }

        private NormalDistributionRandom(double mean, double stndDeviation)
        {
            _mean = mean;
            _stndDeviation = stndDeviation;
        }

    }
}
