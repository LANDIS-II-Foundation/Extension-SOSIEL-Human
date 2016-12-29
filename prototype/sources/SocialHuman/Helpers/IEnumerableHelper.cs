using System;
using System.Collections.Generic;
using System.Linq;


namespace SocialHuman.Helpers
{
    static class IEnumerableHelper
    {
        #region Private fields
        #endregion

        #region Public fields
        #endregion

        #region Constructors
        #endregion

        #region Private methods
        #endregion

        #region Public static methods
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach(T obj in enumerable)
            {
                action(obj);
            }
        }
        #endregion
    }
}
