using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Environments;

    public class Rule
    {
        public RuleLayer Layer { get; set; }

        public int RuleSet { get; set; }

        public int RuleLayer { get; set; }

        public int RulePositionNumber { get; set; }

        public RuleAntecedentPart[] Antecedent { get; set; }

        public RuleConsequent Consequent { get; set; }

        public bool IsAction { get;  set; }
        
        public bool IsModifiable { get; set; }

        public int RequiredParticipants { get;  set; }

        

        public bool IsCollectiveAction
        {
            get
            {
                return RequiredParticipants > 1 || RequiredParticipants == 0;
            }
        }


        public string Id
        {
            get
            {
                return $"RS{Layer.Set.PositionNumber}_L{Layer.PositionNumber}_R{RulePositionNumber}";
            }
        }
        
                
        public bool IsMatch(IAgent agent)
        {
            return Antecedent.All(a=>a.IsMatch(agent));
        }

        //public int CountRightCondition(Func<string, dynamic> func)
        //{
        //    return Antecedent.Count(a => a.IsMatch(func(a.Param)));
        //}

        public void Apply(IAgent agent)
        {
            dynamic value;

            if(string.IsNullOrEmpty(Consequent.VariableValue) == false)
            {
                value = agent[Consequent.VariableValue];
            }
            else
            {
                value = Consequent.Value;
            }

            
            if(Consequent.SavePrevious)
            {
                string key = $"{Agent.VariablesUsedInCode.PreviousPrefix}_{Consequent.Param}";

                agent[key] = agent[Consequent.Param];

                if(Consequent.CopyToCommon)
                {
                    agent.SetToCommon($"{Agent.VariablesUsedInCode.AgentPrefix}_{agent.Id}_{key}", agent[Consequent.Param]);
                }
            }

            if(Consequent.CopyToCommon)
            {
                string key = $"{Agent.VariablesUsedInCode.AgentPrefix}_{agent.Id}_{Consequent.Param}";

                agent.SetToCommon(key, value);
            }

            agent[Consequent.Param] = value;
        }
        
        public static Rule Create(RuleAntecedentPart[] antecedent, RuleConsequent consequent)
        {
            Rule newRule = new Rule();

            newRule.Antecedent = antecedent;
            newRule.Consequent = consequent;
            newRule.IsAction = true;

            return newRule;
        }

        public Rule Copy()
        {
            Rule rule = new Rule();

            rule.Antecedent = Antecedent;
            rule.Consequent = Consequent;
            rule.IsAction = IsAction;
            rule.IsModifiable = IsModifiable;
            rule.RequiredParticipants = RequiredParticipants;

            return rule;
        }
    }
}
