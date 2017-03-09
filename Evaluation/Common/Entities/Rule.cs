using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    public sealed class Rule
    {
        #region Public fields
        //public HeuristicLayer Layer { get; set; }

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


        //public string Id
        //{
        //    get
        //    {
        //        return $"HS{Layer.Set.PositionNumber}_L{Layer.PositionNumber}_H{PositionNumber}";
        //    }
        //}
        #endregion

        #region Private fields
        
        #endregion

        #region Constructors
        private Rule() { }
        #endregion


        #region Public methods
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
        #endregion

        #region Factory methods
        internal static Rule Create(RuleAntecedentPart[] antecedent, RuleConsequent consequent)
        {
            Rule newHeuristic = new Rule();

            newHeuristic.Antecedent = antecedent;
            newHeuristic.Consequent = consequent;
            newHeuristic.FreshnessStatus = 0;
            newHeuristic.IsAction = true;

            return newHeuristic;
        }
        #endregion
    }
}
