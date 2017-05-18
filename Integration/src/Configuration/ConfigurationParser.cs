﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Landis.Extension.SOSIELHuman.Configuration
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

        public static ConfigurationModel ParseConfiguration(string jsonPath)
        {
            if (File.Exists(jsonPath) == false)
                throw new FileNotFoundException(string.Format("Configuration file doesn't found at {0}", jsonPath));

            string jsonContent = File.ReadAllText(jsonPath);

            JToken json = JToken.Parse(jsonContent);

            return json.ToObject<ConfigurationModel>(serializer);
        }
    }


}
