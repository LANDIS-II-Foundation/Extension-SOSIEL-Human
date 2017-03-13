using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class ConfigVariableException : Exception
    {
        readonly string _variableName;
        readonly string _modelName;

        public ConfigVariableException(string variableName, string modelName)
        {
            _variableName = variableName;
            _modelName = modelName;
        }

        public override string ToString()
        {
            return $"{_variableName} variable not found in {_modelName}.configuration.json";
        }
    }
}
