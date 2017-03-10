using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Randoms;

namespace CL1_M1
{


    public class CL1M1Agent : Agent, ICloneableAgent<CL1M1Agent>
    {
        public new CL1M1Agent Clone()
        {
            return (CL1M1Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M1Agent();
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentSubtype] = (AgentSubtype)LinearUniformRandom.GetInstance.Next(1, 3);
        }
    }
}
