using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Configuration;
using Common.Algorithm;

namespace CL1_M1
{
    public class CL1M1Algorithm: IAlgorithm
    {
        readonly Configuration<M1AgentConfiguration> _configuration;

        public string Name { get { return "Cognitive level 1 Model 1"; } }

        public CL1M1Algorithm(Configuration<M1AgentConfiguration> configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
