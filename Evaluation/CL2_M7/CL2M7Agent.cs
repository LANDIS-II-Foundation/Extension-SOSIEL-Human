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

        public List<Goal> AllGoals { get; set; }

        public Dictionary<Goal, GoalState> GoalsState { get; set; } = new Dictionary<Goal, GoalState>();

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
            agent.AllGoals = AllGoals;

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL2M7Agent();
        }

        public void SyncState(IEnumerable<string> assignedRules)
        {
            AssignedRules.Clear();

            Rule[] rules = MentalProto.SelectMany(rs=>rs.AsRuleEnumerable()).Where(r => assignedRules.Contains(r.Id)).ToArray();

            AssignedRules.AddRange(rules);
        }


        public new void GenerateCustomParams()
        {

        }
    }
}
