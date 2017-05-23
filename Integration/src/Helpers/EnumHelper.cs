using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Helpers
{
    public static class EnumHelper
    {
        public static string EnumValueAsString<T>(T value)
        {
            Type t = typeof(T);

            return Enum.GetName(t, value);
        }
    }
}
