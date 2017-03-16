using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M3
{
    public class CL1M3Agent : Agent, ICloneableAgent<CL1M3Agent>
    {
        public new CL1M3Agent Clone()
        {
            return (CL1M3Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M3Agent();
        }

        public new void GenerateCustomParams()
        {
            AgentSubtype agentSubtype = (AgentSubtype)LinearUniformRandom.GetInstance.Next(1, 3);

            this[VariablesUsedInCode.AgentSubtype] = agentSubtype;

            if (agentSubtype == AgentSubtype.Co)
            {
                this[VariablesUsedInCode.AgentC] = this[VariablesUsedInCode.Engage];
            }
            else
            {
                this[VariablesUsedInCode.AgentC] = 0;
            }
        }
    }
}
