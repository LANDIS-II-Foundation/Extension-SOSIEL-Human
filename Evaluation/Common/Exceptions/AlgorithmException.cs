using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class AlgorithmException : Exception
    {
        public AlgorithmException(string message):base(message) { }
    }
}
