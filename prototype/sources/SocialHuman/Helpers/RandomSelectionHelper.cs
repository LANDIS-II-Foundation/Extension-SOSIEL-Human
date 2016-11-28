using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Helpers
{
    using Randoms;

    static class RandomSelectionHelper
    {
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> e) where T : class
        {
            T[] elements = e.ToArray();
            List<int> old = new List<int>();

            for(int i=0; i<elements.Length; i++)
            {
                int randomValue;

                do
                {
                    randomValue = LinearUniformRandom.GetInstance.Next(elements.Length);
                }
                while (old.Contains(randomValue) == true);

                old.Add(randomValue);

                yield return elements[randomValue];
            }
        }
    }
}
