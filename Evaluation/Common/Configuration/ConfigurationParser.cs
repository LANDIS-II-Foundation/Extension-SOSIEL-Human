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
        private class IntConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(int) || objectType == typeof(object));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Integer)
                {
                    return Convert.ToInt32(reader.Value);
                }

                return reader.Value;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }


        static JsonSerializer serializer;

        static ConfigurationParser()
        {
            serializer = new JsonSerializer();

            serializer.Converters.Add(new IntConverter());
        }

        public static AlgorithmConfiguration ParseAlgorithmConfiguration(string jsonContent)
        {
            JToken json = JToken.Parse(jsonContent);

            return json.SelectToken("AlgorithmConfiguration").ToObject<AlgorithmConfiguration>(serializer);
        }

        public static T ParseAgentConfiguration<T>(string jsonContent) where T : class
        {
            JToken json = JToken.Parse(jsonContent);

            return json.SelectToken("AgentConfiguration").ToObject<T>(serializer);
        }

        public static InitialStateConfiguration ParseInitialStateConfiguration(string jsonContent)
        {
            JToken json = JToken.Parse(jsonContent);

            JToken state = json.SelectToken("InitialState");

            if(state != null)
            {
                return state.ToObject<InitialStateConfiguration>(serializer);
            }
            else
            {
                return null;
            }
        }

        public static Configuration<T> ParseConfiguration<T>(string jsonContent) where T : class
        {
            JToken json = JToken.Parse(jsonContent);

            return json.ToObject<Configuration<T>>(serializer);
        }
    }


}
