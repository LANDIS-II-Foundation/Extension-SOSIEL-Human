using System;

namespace SocialHuman.Randoms
{
    class PowerLawRandom
    {
        private static PowerLawRandom random;

        Random r = new Random();

        double power;

        public int Next(double min, double max)
        {
            var x = r.NextDouble();

            return (int)Math.Pow((Math.Pow(max, (power + 1)) - Math.Pow(min, (power + 1))) * x + Math.Pow(min, (power + 1)), (1 / (power + 1)));
        }

        public static PowerLawRandom GetInstance
        {
            get
            {
                if (random == null)
                    random = new PowerLawRandom(Global.Instance.PowerOfDistribution);

                return random;
            }
        }

        private PowerLawRandom(int powerOfDistribution)
        {
            power = powerOfDistribution;
        }
    }
}
