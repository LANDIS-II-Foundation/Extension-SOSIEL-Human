using System;
using System.Collections.Generic;
using System.Linq;

using CL1_M1;


using Common.Algorithm;
using Common.Configuration;


namespace Factory
{
    public static class AlgorithmFactory
    {
        public static IAlgorithm Create(string jsonContent)
        {
            if(string.IsNullOrEmpty(jsonContent))
            {
                throw new ArgumentException("File input.json is empty");
            }

            AlgorithmConfiguration algorithmConfiguration = ConfigurationParser.ParseAlgorithmConfiguration(jsonContent);

            switch (algorithmConfiguration.Model)
            {
                case Common.Enums.Model.M1:
                    {
                        switch(algorithmConfiguration.CognitiveLevel)
                        {
                            case Common.Enums.CognitiveLevel.CL1:
                                {
                                    return new CL1M1Algorithm(ConfigurationParser.ParseConfiguration<M1AgentConfiguration>(jsonContent));
                                }

                            default:
                                throw new NotImplementedException($"Model M{algorithmConfiguration.Model} has unsupported cognitive level {algorithmConfiguration.CognitiveLevel} or it has not implemented  yet");
                        }
                    }

                default:
                    throw new NotImplementedException($"Model M{algorithmConfiguration.Model} has not implemented yet");
            }
        }
    }
}
