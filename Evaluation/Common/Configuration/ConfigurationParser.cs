using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace Common.Configuration
{
    public static class ConfigurationParser
    {
        static JsonSerializer serializer;

        static ConfigurationParser()
        {
            var contractResolver = new DefaultContractResolver();
            contractResolver.NamingStrategy = new DefaultNamingStrategy();

            serializer = new JsonSerializer();

            serializer.ContractResolver = contractResolver;
            serializer.Formatting = Formatting.None;
        }

        public static AlgorithmConfiguration ParseAlgorithmConfiguration(string jsonContent)
        {
            JToken json = JToken.Parse(jsonContent);

            return json.SelectToken("AlgorithmConfiguration").ToObject<AlgorithmConfiguration>(serializer);
        }

        public static Configuration<T> ParseConfiguration<T>(string jsonContent) where T : class
        {
            return JsonConvert.DeserializeObject<Configuration<T>>(jsonContent);
        }
    }
}
