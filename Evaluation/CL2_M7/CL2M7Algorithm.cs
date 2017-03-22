using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using Common.Configuration;
using Common.Algorithm;
using Common.Entities;
using Common.Helpers;
using Common.Randoms;
using Common.Models;

namespace CL1_M6
{
    public sealed class CL2M7Algorithm :  IAlgorithm
    {
        public string Name { get { return "Cognitive level 2 Model 7"; } }

        private string _outputFolder;

        readonly Configuration<CL2M7Agent> _configuration;

        public CL2M7Algorithm(Configuration<CL2M7Agent> configuration)
        {
            _configuration = configuration;

            _outputFolder = @"Output\CL2_M7";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);
        }

        private void Initialize()
        {
            
        }

        private void ExecuteAlgorithm()
        {
            throw new NotImplementedException();
        }

        
        private double CalculateAgentWellbeing(IAgent agent, Site centerSite)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Run()
        {
            Initialize();





            return _outputFolder;
        }
    }
}
