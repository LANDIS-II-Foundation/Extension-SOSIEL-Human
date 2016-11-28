using System;

namespace SocialHuman.Randoms
{
    class PowerLawRandom
    {
        private static PowerLawRandom random = new PowerLawRandom();

        Random r = new Random();

        double t = 4d;

        public int Next(double min, double max)
        {
            var x = r.NextDouble();

            return (int)Math.Pow((Math.Pow(max, (t + 1)) - Math.Pow(min, (t + 1))) * x + Math.Pow(min, (t + 1)), (1 / (t + 1)));
        }

        public static PowerLawRandom GetInstance
        {
            get
            {
                return random;
            }
        }

        private PowerLawRandom() { }
    }
}
