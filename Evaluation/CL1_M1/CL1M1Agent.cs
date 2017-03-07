using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Environment;


namespace CL1_M1
{


    public class CL1M1Agent : Agent, ICloneable<CL1M1Agent>
    {
        public new CL1M1Agent Clone()
        {
            return (CL1M1Agent)base.Clone();
        }

    }
}
