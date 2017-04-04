﻿using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Common.Entities
{
    using Configuration;
    using Exceptions;
    using Helpers;

    public class Agent : IAgent, ICloneableAgent<Agent>
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
            //public const string MaxEngage = "MaxE";
            public const string Endowment = "E";
            public const string MagnitudeOfExternalities = "M";
            public const string CommonPoolSize = "CommonPoolSize";
            public const string CommonPoolSubtupeProportion = "CommonPoolSubtupeProportion";
            public const string CommonPoolC = "CommonPoolC";
            public const string AgentSiteWellbeing = "AgentSiteWellbeing";

            //M3
            public const string InitialDisturbance = "InitialDisturbance";
            public const string DisturbanceIncrement = "DisturbanceIncrement";

            //M4
            //public const string MaxPunishment = "MaxP";
            public const string Punishment = "P";
            public const string AgentP = "AgentP";


            //M6
            public const string ResourceMax = "ResourceMax";


            //M7
            public const string Iteration = "Iteration";
            public const string AgentWellbeing = "AgentWellbeing";
            public const string AgentPrefix = "Agent";
            public const string PreviousPrefix = "Previous";


            //M8
            public const string PoolWellbeing = "PoolWellbeing";


            //M11
            public const string AgentE = "AgentE";
            public const string AgentSavings = "AgentSavings";
            public const string P = "P";
            public const string R = "R";
            public const string TotalEndowment = "TotalE";
        }

        public int Id { get; set; }

        public string PrototypeName { get; set; }

        public Dictionary<string, RuleSetSettings> SetSettings { get; set; }

        public List<Goal> Goals { get; set; }

        public List<Rule> AssignedRules { get; set; }

        public List<IAgent> ConnectedAgents { get; set; }

        [JsonProperty()]
        protected Dictionary<string, dynamic> Variables { get; set; } = new Dictionary<string, dynamic>();

        protected List<RuleSet> MentalProto
        {
            get
            {
                return _mentalProto == null ? _mentalProto = TransformRulesToRuleSets() : _mentalProto;
            }
        }

        [JsonProperty("Rules")]
        private List<Rule> Rules { get; set; }

        private List<RuleSet> _mentalProto;

        private bool UseDoNothing;

        private void AddDoNothingRules()
        {
            _mentalProto.ForEach(s =>
            {
                s.Layers.ForEach(l =>
                {
                    if (!l.Rules.Any(r => r.IsAction == false))
                    {
                        Rule doNothing = new Rule
                        {
                            Antecedent = new RuleAntecedentPart[] { new RuleAntecedentPart { Param = VariablesUsedInCode.AgentStatus, Sign = "==", Value = "active" } },
                            Consequent = new RuleConsequent { Param = VariablesUsedInCode.AgentC, Value = 0, CopyToCommon = true, SavePrevious = true },
                            IsAction = false
                        };

                        l.Add(doNothing);
                    }
                });
            });
        }

        private List<RuleSet> TransformRulesToRuleSets()
        {
            if (_mentalProto != null)
                return _mentalProto;

            _mentalProto = Rules.GroupBy(r => r.RuleSet).OrderBy(g => g.Key).Select(g =>
                   new RuleSet(g.Key, Goals.Where(goal => SetSettings[g.Key.ToString()].AssociatedWith.Contains(goal.Name)).ToArray(),
                       g.GroupBy(r => r.RuleLayer).OrderBy(g2 => g2.Key).Select(g2 => new RuleLayer(SetSettings[g.Key.ToString()].Layer[g2.Key.ToString()], g2)))).ToList();

            AddDoNothingRules();

            return _mentalProto;
        }

        public virtual dynamic this[string key]
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

            agent.PrototypeName = PrototypeName;

            //common variables
            agent.Variables = Variables;
            
            agent.Rules = Rules;
            agent.Goals = Goals;
            agent.SetSettings = SetSettings;
            agent._mentalProto = _mentalProto;
            agent.AssignedRules = new List<Rule>(AssignedRules);
            agent.ConnectedAgents = new List<IAgent>();

            if (agent.Variables.ContainsKey(VariablesUsedInCode.AgentCurrentSite))
                agent.Variables.Remove(VariablesUsedInCode.AgentCurrentSite);

            return agent;
        }

        protected virtual Agent CreateInstance()
        {
            return new Agent();
        }

        public void GenerateCustomParams()
        {

        }

        public void SetToCommon(string key, dynamic value)
        {
            Variables[key] = value;
        }

        public void AssignRules(IEnumerable<string> assignedRules)
        {
            AssignedRules.Clear();

            Rule[] allRules = MentalProto.SelectMany(rs => rs.AsRuleEnumerable()).ToArray();

            Rule[] initialRules = allRules.Where(r => assignedRules.Contains(r.Id)).ToArray();

            RuleLayer[] layers = initialRules.Select(r => r.Layer).Distinct().ToArray();

            Rule[] additionalDoNothingRules = allRules.Where(r => r.IsAction == false && layers.Any(l => r.Layer == l)).ToArray();

            AssignedRules.AddRange(initialRules);
            AssignedRules.AddRange(additionalDoNothingRules);
        }
    }
}
