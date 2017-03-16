using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M4
{
    public class CL1M4Agent : Agent, ICloneableAgent<CL1M4Agent>
    {
        public new CL1M4Agent Clone()
        {
            return (CL1M4Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M4Agent();
        }

        public new void GenerateCustomParams()
        {
            int randomType = LinearUniformRandom.GetInstance.Next(1, 4);

            AgentSubtype subtype = (AgentSubtype)randomType;

            this[VariablesUsedInCode.AgentSubtype] = subtype;

            if (subtype == AgentSubtype.Co || subtype == AgentSubtype.Enf)
            {
                this[VariablesUsedInCode.AgentC] = this[VariablesUsedInCode.Engage];

                if (subtype == AgentSubtype.Enf)
                {
                    this[VariablesUsedInCode.AgentP] = this[VariablesUsedInCode.Punishment];
                }
            }
            else
            {
                this[VariablesUsedInCode.AgentC] = 0;
                this[VariablesUsedInCode.AgentP] = 0;
            }
        }
    }
}
