using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M2
{
    public class CL1M2Agent : Agent, ICloneableAgent<CL1M2Agent>
    {
        public new CL1M2Agent Clone()
        {
            return (CL1M2Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M2Agent();
        }

        public new void GenerateCustomParams()
        {
            
        }
    }
}
