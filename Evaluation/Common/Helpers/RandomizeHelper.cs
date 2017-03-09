using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Helpers
{
    using Randoms;

    public static class RandomizeHelper
    {
        public static T RandomizeOne<T>(this IEnumerable<T> source)
        {
            T[] temp = source.ToArray();

            int position = LinearUniformRandom.GetInstance.Next(temp.Length);

            return temp[position];
        }


        public static List<T> Randomize<T>(List<T> original)
        {
            List<T> temp = new List<T>(original.Count);
            List<T> result = new List<T>(original.Count);


            temp.AddRange(original);

            while(temp.Count > 0)
            {
                T item = temp[LinearUniformRandom.GetInstance.Next(temp.Count)];

                result.Add(item);
                temp.Remove(item);
            }


            return result;
        }
    }
}
