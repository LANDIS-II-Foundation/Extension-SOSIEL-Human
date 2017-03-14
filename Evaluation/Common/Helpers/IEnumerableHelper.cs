 using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Helpers
{
    static class IEnumerableHelper
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach(T obj in enumerable)
            {
                action(obj);
            }
        }
    }
}
