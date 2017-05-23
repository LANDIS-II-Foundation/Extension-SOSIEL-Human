using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Exceptions
{
    public class UnknownVariableException : Exception
    {
        readonly string variableName;

        public UnknownVariableException(string variableName)
        {
            this.variableName = variableName;
        }

        public override string ToString()
        {
            return $"{variableName} wasn't defined for agent. Maybe you forgot to define it in config";
        }
    }
}
