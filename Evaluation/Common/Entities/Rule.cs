using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    public sealed class Rule
    {
        
        public RuleLayer Layer { get; set; }

        public int RuleSet { get; set; }

        public int RuleLayer { get; set; }

        public int RulePositionNumber { get; set; }

        public RuleAntecedentPart[] Antecedent { get; set; }

        public RuleConsequent Consequent { get; set; }

        public int FreshnessStatus { get; set; }

        public bool IsAction { get;  set; }
               
        public int RequiredParticipants { get;  set; }

        public bool IsCollectiveAction
        {
            get
            {
                return RequiredParticipants > 1;
            }
        }


        public string Id
        {
            get
            {
                return $"RS{Layer.Set.PositionNumber}_L{Layer.PositionNumber}_R{RulePositionNumber}";
            }
        }
        
        private Rule() { }
        
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
            if(string.IsNullOrEmpty(Consequent.VariableValue) == false)
            {
                agent[Consequent.Param] = agent[Consequent.VariableValue];
            }
            else
            {
                agent[Consequent.Param] = Consequent.Value;
            }
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
