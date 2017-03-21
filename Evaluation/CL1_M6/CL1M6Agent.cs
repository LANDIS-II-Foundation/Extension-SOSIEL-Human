using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M6
{
    public class CL1M6Agent : Agent, ICloneableAgent<CL1M6Agent>
    {
        public new CL1M6Agent Clone()
        {
            return (CL1M6Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M6Agent();
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
