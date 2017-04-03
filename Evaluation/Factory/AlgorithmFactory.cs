using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


//using CL1_M1;
//using CL1_M2;
//using CL1_M3;
//using CL1_M4;
//using CL1_M5;
//using CL1_M6;
//using CL2_M7;
//using CL2_M8;
//using CL2_M9;
//using CL3_M10;
using CL4_M11;

using Common.Algorithm;
using Common.Configuration;
using Common.Enums;

namespace Factory
{
    public static class AlgorithmFactory
    {
        private static Dictionary<string,T> ReadModelConfig<T>() where T : class
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

        private static InitialStateConfiguration ReadInitialConfig(Type agentType)
        {
            string configFileName = $"{agentType.Name}.configuration.json";

            string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), configFileName);

            if (File.Exists(configFilePath) == false)
            {
                throw new FileNotFoundException($"{configFileName} not found at {Directory.GetCurrentDirectory()}");
            }

            return ConfigurationParser.ParseInitialStateConfiguration(File.ReadAllText(configFilePath));
        }

        private static Configuration<T> JoinConfigs<T>(AlgorithmConfiguration algorithm, Dictionary<string, T> agent) where T : class
        {
            return new Configuration<T> { AgentConfiguration = agent, AlgorithmConfiguration = algorithm };
        }

        private static Configuration<T> JoinConfigs<T>(AlgorithmConfiguration algorithm, Dictionary<string, T> agent, InitialStateConfiguration initialState) where T : class
        {
            return new Configuration<T> { AgentConfiguration = agent, AlgorithmConfiguration = algorithm, InitialState = initialState };
        }

        public static IAlgorithm Create(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                throw new ArgumentException("File algorithm.json is empty");
            }

            AlgorithmConfiguration algorithmConfig = ConfigurationParser.ParseAlgorithmConfiguration(jsonContent);


            switch (algorithmConfig.Model)
            {
                //case Model.M1:
                //    {
                //        Configuration<CL1M1Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M1Agent>());

                //        return new CL1M1Algorithm(config);
                //    }
                //case Model.M2:
                //    {
                //        Configuration<CL1M2Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M2Agent>());

                //        return new CL1M2Algorithm(config);
                //    }

                //case Model.M3:
                //    {
                //        Configuration<CL1M3Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M3Agent>());

                //        return new CL1M3Algorithm(config);
                //    }

                //case Model.M4:
                //    {
                //        Configuration<CL1M4Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M4Agent>());

                //        return new CL1M4Algorithm(config);
                //    }

                //case Model.M5:
                //    {
                //        Configuration<CL1M5Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M5Agent>());

                //        return new CL1M5Algorithm(config);
                //    }

                //case Model.M6:
                //    {
                //        Configuration<CL1M6Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL1M6Agent>());

                //        return new CL1M6Algorithm(config);
                //    }

                //case Model.M7:
                //    {
                //        Configuration<CL2M7Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL2M7Agent>(), ReadInitialConfig(typeof(CL2M7Agent)));

                //        return new CL2M7Algorithm(config);
                //    }

                //case Model.M8:
                //    {
                //        Configuration<CL2M8Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL2M8Agent>(), ReadInitialConfig(typeof(CL2M8Agent)));

                //        return new CL2M8Algorithm(config);
                //    }

                //case Model.M9:
                //    {
                //        Configuration<CL2M9Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL2M9Agent>(), ReadInitialConfig(typeof(CL2M9Agent)));

                //        return new CL2M9Algorithm(config);
                //    }

                //case Model.M10:
                //    {
                //        Configuration<CL3M10Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL3M10Agent>(), ReadInitialConfig(typeof(CL3M10Agent)));

                //        return new CL3M10Algorithm(config);

                //    }

                case Model.M10:
                    {
                        Configuration<CL4M11Agent> config = JoinConfigs(algorithmConfig, ReadModelConfig<CL4M11Agent>(), ReadInitialConfig(typeof(CL4M11Agent)));

                        return new CL4M11Algorithm(config);

                    }


                default:
                    throw new NotImplementedException($"Model {algorithmConfig.Model} hasn't implemented  yet");
            }
        }
    }
}

