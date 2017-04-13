using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Common.Entities
{
    using Common.Enums;
    using Configuration;
    using Exceptions;
    using Helpers;

    public partial class Agent : IAgent, ICloneableAgent<Agent>
    {
        public override string ToString()
        {
            return EnumHelper.EnumValueAsString(this[VariablesUsedInCode.AgentSubtype]);
        }

        public int Id { get; set; }

        public string PrototypeName { get; set; }

        public Dictionary<string, RuleSetSettings> SetSettings { get; set; }

        public List<Goal> Goals { get; set; }

        public List<Rule> AssignedRules { get; set; } = new List<Rule>();

        public Dictionary<Rule, int> RuleActivationFreshness { get; set; } = new Dictionary<Rule, int>();


        public List<IAgent> ConnectedAgents { get; set; }

        public AgentStateConfiguration InitialStateConfiguration { get; set; }

        public SocialNetworkType SocialNetwork { get; set; }

        

        public IEnumerable<Rule> MentalModelRules
        {
            get
            {
                return _mentalProto.SelectMany(s => s.AsRuleEnumerable());
            }
        }

        [JsonProperty]
        protected Dictionary<string, dynamic> Variables { get; set; } = new Dictionary<string, dynamic>();

        protected List<RuleSet> MentalProto
        {
            get
            {
                return _mentalProto == null ? _mentalProto = TransformRulesToRuleSets() : _mentalProto;
            }
        }

        [JsonProperty]
        private List<Rule> Rules;

        private List<RuleSet> _mentalProto;


        [JsonProperty]
        private bool UseDoNothing;

        private void AddDoNothingRules()
        {
            _mentalProto.ForEach(s =>
            {
                s.Layers.ForEach(l =>
                {
                    if (!l.Rules.Any(r => r.IsAction == false))
                    {
                        Rule proto = l.Rules.First();

                        Rule doNothing = new Rule
                        {
                            Antecedent = new RuleAntecedentPart[] { new RuleAntecedentPart { Param = VariablesUsedInCode.AgentStatus, Sign = "==", Value = "active" } },
                            RequiredParticipants = 1,
                            IsAction = false
                        };

                        if(proto.Consequent.Param == VariablesUsedInCode.AgentCurrentSite)
                        {
                            doNothing.Consequent = new RuleConsequent
                            {
                                Param = VariablesUsedInCode.AgentStatus,
                                VariableValue = VariablesUsedInCode.AgentStatus
                            };
                        }
                        else
                        {
                            doNothing.Consequent = new RuleConsequent
                            {
                                Param = proto.Consequent.Param,
                                Value = 0,
                                CopyToCommon = proto.Consequent.CopyToCommon,
                                SavePrevious = proto.Consequent.SavePrevious
                            };
                        }

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
                       g.GroupBy(r => r.RuleLayer).OrderBy(g2 => g2.Key).Select(g2 => new RuleLayer(SetSettings[g.Key.ToString()].Layer[g2.Key.ToString()], g2)), SetSettings[g.Key.ToString()].IsSequential)).ToList();

            if (UseDoNothing)
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

        public virtual bool ContainsVariable(string key)
        {
            return Variables.ContainsKey(key);
        }

        protected virtual void PostSetValue(string variable, dynamic newValue)
        {
            if (variable == VariablesUsedInCode.AgentCurrentSite)
            {
                if (newValue != null)
                {
                    Site newSite = (Site)newValue;
                    newSite.OccupiedBy = this;
                    newSite.IsOccupationChanged = true;
                    newSite.SiteList.AdjacentSites(newSite).ForEach(s => s.IsOccupationChanged = true);
                }
            }
        }

        protected virtual void PreSetValue(string variable, dynamic oldValue)
        {
            if (variable == VariablesUsedInCode.AgentCurrentSite)
            {
                Site oldSite = (Site)oldValue;
                
                oldSite.OccupiedBy = null;
                oldSite.IsOccupationChanged = true;
                oldSite.SiteList.AdjacentSites(oldSite).ForEach(s => s.IsOccupationChanged = true);
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
            agent.RuleActivationFreshness = new Dictionary<Rule, int>(RuleActivationFreshness);
            agent.ConnectedAgents = new List<IAgent>();
            agent.UseDoNothing = UseDoNothing;
            agent.InitialStateConfiguration = InitialStateConfiguration;
            agent.SocialNetwork = SocialNetwork;

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

        public void AssignInitialRules(IEnumerable<string> assignedRules)
        {
            AssignedRules.Clear();
            RuleActivationFreshness.Clear();

            Rule[] allRules = MentalProto.SelectMany(rs => rs.AsRuleEnumerable()).ToArray();

            Rule[] initialRules = allRules.Where(r => assignedRules.Contains(r.Id)).ToArray();

            RuleLayer[] layers = initialRules.Select(r => r.Layer).Distinct().ToArray();

            Rule[] additionalDoNothingRules = allRules.Where(r => r.IsAction == false && layers.Any(l => r.Layer == l)).ToArray();


            Rule[] allInitialRules = Enumerable.Concat(initialRules, additionalDoNothingRules).ToArray();

            allInitialRules.ForEach(r =>
            {
                RuleActivationFreshness.Add(r, 1);
            });

            AssignedRules.AddRange(allInitialRules);
        }

        public void AssignNewRule(Rule newRule)
        {
            RuleLayer layer = newRule.Layer;

            Rule[] layerRules = AssignedRules.GroupBy(r => r.Layer).Where(g => g.Key == layer).SelectMany(g => g).ToArray();

            if(layerRules.Length < layer.LayerSettings.MaxNumberOfRules)
            {
                AssignedRules.Add(newRule);

                RuleActivationFreshness.Add(newRule, 0);
            }
            else
            {
                Rule ruleForRemoving = RuleActivationFreshness.Where(kvp => kvp.Key.Layer == layer && kvp.Key.IsAction).GroupBy(kvp => kvp.Value).OrderByDescending(g => g.Key)
                    .Take(1).SelectMany(g => g.Select(kvp => kvp.Key)).RandomizeOne();

                AssignedRules.Remove(ruleForRemoving);

                RuleActivationFreshness.Remove(ruleForRemoving);

                AssignNewRule(newRule);
            }
        }
    }
}
