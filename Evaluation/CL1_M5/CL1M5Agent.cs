using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M5
{
    public class CL1M5Agent : Agent, ICloneableAgent<CL1M5Agent>
    {
        public new CL1M5Agent Clone()
        {
            return (CL1M5Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M5Agent();
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentC] = LinearUniformRandom.GetInstance.Next(this[VariablesUsedInCode.Engage] + 1);
            this[VariablesUsedInCode.AgentP] = LinearUniformRandom.GetInstance.Next(this[VariablesUsedInCode.Engage] + 1);

            if (this[VariablesUsedInCode.AgentC] + this[VariablesUsedInCode.AgentP] == 0)
                this[VariablesUsedInCode.AgentSubtype] = AgentSubtype.NonCo;
            else
                this[VariablesUsedInCode.AgentSubtype] = AgentSubtype.Unknown;
        }
    }
}
