using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Configuration;
using Common.Algorithm;
using Common.Entities;

namespace CL1_M1
{
    public class CL1M1Algorithm: IAlgorithm
    {
        readonly Configuration<CL1M1Agent> _configuration;

        public string Name { get { return "Cognitive level 1 Model 1"; } }

        public CL1M1Algorithm(Configuration<CL1M1Agent> configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            SiteList siteList = SiteList.Generate(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AlgorithmConfiguration.VacantProportion);

            AgentList agentList = AgentList.Generate(_configuration.AlgorithmConfiguration.AgentCount, _configuration.AgentConfiguration, siteList);
        }


        private static void SaveInitialState()
        {

        }

        private static void SaveFinalState()
        {

        }
    }
}
