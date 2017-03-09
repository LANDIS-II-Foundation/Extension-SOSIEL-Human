using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environment;

    public class Agent : ICloneableAgent<Agent>
    {
        public static class VariablesUsedInCode
        {
            //Common
            public const string AgentType = "AgentType";
            public const string AgentStatus = "AgentStatus";
            public const string AgentSite = "AgentSite";
            //public const string NeighborhoodSize = "NeighborhoodSize";
            public const string NeighborhoodVacantSites = "NeighborhoodVacantSites";
            public const string AgentBetterSite = "AgentBetterSite";
            public const string AgentBetterSiteAvailable = "AgentBetterSiteAvailable";


            //M1
            public const string AgentSubtype = "AgentSubtype";
            public const string NeighborhoodUnalike = "NeighborhoodUnalike";
            public const string NeighborhoodSubtypeProportion = "NeighborhoodSubtypeProportion";
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
            Agent agent = CreateInstance();

            agent.Variables = new Dictionary<string, dynamic>(Variables);
            agent.Rules = Rules;

            return agent;
        }

        protected virtual Agent CreateInstance()
        {
            return new Agent();
        }

        public void GenerateCustomParams()
        {
            
        }
    }
}
