using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    public class Rule
    {
        
        public RuleLayer Layer { get; set; }

        public int RuleSet { get; set; }

        public int RuleLayer { get; set; }

        public int RulePositionNumber { get; set; }

        public RuleAntecedentPart[] Antecedent { get; set; }

        public RuleConsequent Consequent { get; set; }

        public int FreshnessStatus { get; set; }

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
        
        internal static Rule Create(RuleAntecedentPart[] antecedent, RuleConsequent consequent)
        {
            Rule newHeuristic = new Rule();

            newHeuristic.Antecedent = antecedent;
            newHeuristic.Consequent = consequent;
            newHeuristic.FreshnessStatus = 0;
            newHeuristic.IsAction = true;

            return newHeuristic;
        }
        
    }
}
