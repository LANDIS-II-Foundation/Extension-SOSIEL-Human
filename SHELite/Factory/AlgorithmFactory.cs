using Common.Algorithm;
using SHELite;
using SHELite.Configuration;

namespace Factory
{
    public static class AlgorithmFactory
    {
        public static IAlgorithm Create(string path)
        {
            ConfigurationModel algorithmConfig = ConfigurationParser.ParseConfiguration(path);

            return new SHELiteAlgorithm(algorithmConfig);
        }
    }
}

