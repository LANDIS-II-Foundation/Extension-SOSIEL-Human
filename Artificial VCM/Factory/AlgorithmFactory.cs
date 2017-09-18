using System;
using System.IO;
using ArtificialVCM;
using ArtificialVCM.Configuration;
using Common.Algorithm;
using Common.Configuration;
using Common.Enums;

namespace Factory
{
    public static class AlgorithmFactory
    {
        public static IAlgorithm Create(string path)
        {
            ConfigurationModel algorithmConfig = ConfigurationParser.ParseConfiguration(path);

            return new ArtificialVCMAlgorithm(algorithmConfig);
        }
    }
}

