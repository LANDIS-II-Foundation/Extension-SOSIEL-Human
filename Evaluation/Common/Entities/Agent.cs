using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Common.Entities
{
    using Exceptions;

    public class Agent : ICloneableAgent<Agent>
    {
        public static class VariablesUsedInCode
        {
            //Common
            public const string AgentType = "AgentType";
            public const string AgentStatus = "AgentStatus";
            public const string AgentCurrentSite = "AgentCurrentSite";
            public const string NeighborhoodSize = "NeighborhoodSize";
            public const string NeighborhoodVacantSites = "NeighborhoodVacantSites";
            public const string AgentBetterSite = "AgentBetterSite";
            public const string AgentBetterSiteAvailable = "AgentBetterSiteAvailable";
            public const string AgentSubtype = "AgentSubtype";
            public const string NeighborhoodUnalike = "NeighborhoodUnalike";
            
            //M1
            
            public const string NeighborhoodSubtypeProportion = "NeighborhoodSubtypeProportion";

            //M2
            public const string AgentC = "AgentC";
            public const string MaxE = "MaxE";
            public const string E = "E";
            public const string M = "M";
            public const string CommonPoolSize = "CommonPoolSize";
            public const string CommonPoolSubtupeProportion = "CommonPoolSubtupeProportion";
            public const string CommonPoolC = "CommonPoolC";
            public const string AgentSiteWellbeing = "AgentSiteWellbeing";

            //M3
            public const string InitialDisturbance = "InitialDisturbance";
            public const string DisturbanceIncrement = "DisturbanceIncrement";
        }


        public int Id { get; set; }

        [JsonProperty("Variables")]
        protected Dictionary<string, dynamic> Variables { get; set; }

        public List<Rule> Rules { get; set; }

        public dynamic this[string key]
        {
            get
            {
                if (Variables.ContainsKey(key))
                    return Variables[key];

                throw new ConfigVariableException(key, GetType().Name);
            }
            set
            {
                if (Variables.ContainsKey(key))
                    PreSetValue(key, Variables[key]);

                Variables[key] = value;

                PostSetValue(key, value);
            }

        }

        protected virtual void PostSetValue(string variable, dynamic newValue)
        {
            if (variable == VariablesUsedInCode.AgentCurrentSite)
            {
                if (newValue != null)
                {
                    Site newSite = (Site)newValue;
                    newSite.OccupiedBy = this;
                }
            }
        }

        protected virtual void PreSetValue(string variable, dynamic oldValue)
        {
            if (variable == VariablesUsedInCode.AgentCurrentSite)
            {
                Site oldSite = (Site)oldValue;
                oldSite.OccupiedBy = null;
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
