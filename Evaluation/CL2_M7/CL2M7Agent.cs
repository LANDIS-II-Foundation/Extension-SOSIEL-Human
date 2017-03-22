using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M6
{
    public class CL2M7Agent : Agent, ICloneableAgent<CL2M7Agent>, IAnticipatedInfluenceState
    {
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }
        

        public new CL2M7Agent Clone()
        {
            return (CL2M7Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL2M7Agent();
        }

        public new void GenerateCustomParams()
        {

        }
    }
}
