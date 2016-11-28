using System;

namespace SocialHuman.Randoms
{
    public sealed class LinearUniformRandom
    {
        private static Random random = new Random();

        public static Random GetInstance
        {
            get
            {
                return random;
            }
        }

        private LinearUniformRandom() { }
    }
}
