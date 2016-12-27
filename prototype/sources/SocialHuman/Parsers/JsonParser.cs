using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SocialHuman.Parsers
{
    using Models;

    class JsonParser : IParser
    {
        JToken json;

        public JsonParser(string filePath)
        {
            if (File.Exists(filePath) == false)
                throw new FileNotFoundException("input.json not found");


            using (JsonTextReader jtr = new JsonTextReader(new StreamReader(filePath)))
            {
                json = JToken.ReadFrom(jtr);
            }
        }

        public Dictionary<string, ActorPrototype> ParseActorPrototypes()
        {
            return json.SelectToken("actors").ToObject<Dictionary<string, ActorPrototype>>();
        }

        public GlobalInput ParseGlogalConfiguration()
        {
            return json.SelectToken("global_variables").ToObject<GlobalInput>();
        }

        public Dictionary<string, InitialState> ParseInitialState()
        {
            return json.SelectToken("initial_state").ToObject<Dictionary<string, InitialState>>();
        }

    }
}
