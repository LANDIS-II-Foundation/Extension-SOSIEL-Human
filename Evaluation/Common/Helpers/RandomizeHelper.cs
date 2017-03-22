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
            return RandomizeOne(source.ToArray());
        }

        public static T RandomizeOne<T>(this T[] source)
        {
            try
            {
                int position = LinearUniformRandom.GetInstance.Next(source.Length);

                return source[position];
            }
            catch(Exception ex)
            {

            }

            return source[0];
        }

        public static T RandomizeOne<T>(this List<T> source)
        {
            int position = LinearUniformRandom.GetInstance.Next(source.Count);

            return source[position];
        }

        public static List<T> Randomize<T>(IEnumerable<T> original)
        {
            List<T> temp = new List<T>();
            List<T> result = new List<T>();


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
