using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


using CL1_M1;
using CL1_M2;


using Common.Algorithm;
using Common.Configuration;


namespace Factory
{
    public static class AlgorithmFactory
    {
        private static T ReadModelConfig<T>() where T : class
        {
            Type type = typeof(T);

            string configFileName = $"{type.Name}.configuration.json";

            string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), configFileName);

            if (File.Exists(configFilePath) == false)
            {
                throw new FileNotFoundException($"{configFileName} not found at {Directory.GetCurrentDirectory()}");
            }

            return ConfigurationParser.ParseAgentConfiguration<T>(File.ReadAllText(configFilePath));
        }

        private static Configuration<T> JoinConfigs<T>(AlgorithmConfiguration algorithm, T agent) where T : class
        {
            return new Configuration<T> { AgentConfiguration = agent, AlgorithmConfiguration = algorithm };
        }

        public static IAlgorithm Create(string jsonContent)
        {
            if(string.IsNullOrEmpty(jsonContent))
            {
                throw new ArgumentException("File algorithm.json is empty");
            }

            AlgorithmConfiguration algorithmConfig = ConfigurationParser.ParseAlgorithmConfiguration(jsonContent);

            
            switch (algorithmConfig.Model)
            {
                case Common.Enums.Model.M1:
                    {
                        switch(algorithmConfig.CognitiveLevel)
                        {
                            case Common.Enums.CognitiveLevel.CL1:
                                {
                                    Configuration<CL1M1Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M1Agent>());

                                    return new CL1M1Algorithm(config);
                                }
                            default:
                                throw new NotImplementedException($"Model M{algorithmConfig.Model} has unsupported cognitive level {algorithmConfig.CognitiveLevel} or it hasn't implemented  yet");
                        }
                    }
                case Common.Enums.Model.M2:
                    {
                        switch (algorithmConfig.CognitiveLevel)
                        {
                            case Common.Enums.CognitiveLevel.CL1:
                                {
                                    Configuration<CL1M2Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M2Agent>());

                                    return new CL1M2Algorithm(config);
                                }
                            default:
                                throw new NotImplementedException($"Model M{algorithmConfig.Model} has unsupported cognitive level {algorithmConfig.CognitiveLevel} or it hasn't implemented  yet");
                        }
                    }

                default:
                    throw new NotImplementedException($"Model M{algorithmConfig.Model} has not implemented yet");
            }
        }
    }
}
