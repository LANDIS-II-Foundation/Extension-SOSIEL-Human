using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    public class TakenAction
    {
        public string HeuristicId { get; private set; }

        public string VariableName { get; private set; }

        public dynamic Value { get; private set; }


        public TakenAction(string heuristicId, string variableName, dynamic value)
        {
            HeuristicId = heuristicId;
            VariableName = variableName;
            Value = value;
        }

    }
}
