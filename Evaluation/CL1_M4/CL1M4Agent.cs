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
            
        }
    }
}
