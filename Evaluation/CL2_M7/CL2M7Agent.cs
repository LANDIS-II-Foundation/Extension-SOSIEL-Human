using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Helpers;
using Common.Exceptions;
using Common.Randoms;

namespace CL2_M7
{
    public sealed class CL2M7Agent : Agent, ICloneableAgent<CL2M7Agent>, IConfigurableAgent
    {
        public List<Rule> AssignedRules { get; set; } = new List<Rule>();

        public List<RuleSet> MentalProto { get; set; }

        

        private Dictionary<string, dynamic> PrivateVariables { get; set; } = new Dictionary<string, dynamic>();



        public override dynamic this[string key]
        {
            get
            {
                if (PrivateVariables.ContainsKey(key))
                    return PrivateVariables[key];
                else
                {
                    return base[key];
                }
            }

            set
            {
                if (Variables.ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    PrivateVariables[key] = value;
                }

            }
        }

        public new CL2M7Agent Clone()
        {
            CL2M7Agent agent = (CL2M7Agent)base.Clone();

            agent.AssignedRules = new List<Rule>(AssignedRules);
            agent.MentalProto = TransformRulesToRuleSets();        
            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL2M7Agent();
        }

        public void SyncState(IEnumerable<string> assignedRules)
        {
            AssignedRules.Clear();

            Rule[] allRules = MentalProto.SelectMany(rs => rs.AsRuleEnumerable()).ToArray();

            Rule[] initialRules = allRules.Where(r => assignedRules.Contains(r.Id)).ToArray();

            RuleLayer[] layers = initialRules.Select(r => r.Layer).Distinct().ToArray();

            Rule[] additionalDoNothingRules = allRules.Where(r => r.IsAction == false && layers.Any(l => r.Layer == l)).ToArray();

            AssignedRules.AddRange(initialRules);
            AssignedRules.AddRange(additionalDoNothingRules);
        }


        public void SetToCommon(string key, dynamic value)
        {
            Variables[key] = value;
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentC] = 0;
            this[VariablesUsedInCode.AgentWellbeing] = 0;
            this[$"{VariablesUsedInCode.PreviousPrefix}_{VariablesUsedInCode.AgentC}"] = 0;
        }
    }
}
