using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environment;

    public class Agent : ICloneable<Agent>
    {
        public static class UsingInCodeVariables
        {
            public const string AgentSubtype = "AgentSubtype";
            public const string AgentStatus = "AgentStatus";
            public const string AgentSite = "AgentSite";
        }


        public int Id { get; set; }

        public Dictionary<string, dynamic> Variables { get; set; }

        public List<Rule> Rules { get; set; }

        public dynamic this[string key]
        {
            get
            {
                if (Variables.ContainsKey(key))
                    return Variables[key];

                throw new ArgumentException($"Unknown agent variable: {key}");
            }
            set
            {

                Variables[key] = value;
            }

        }

        public virtual Agent Clone()
        {
            Agent agent = new Agent();

            agent.Variables = new Dictionary<string, dynamic>(Variables);
            agent.Rules = Rules;

            return agent;
        }
    }
}
