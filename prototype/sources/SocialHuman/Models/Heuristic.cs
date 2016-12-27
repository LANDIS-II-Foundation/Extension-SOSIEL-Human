using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace SocialHuman.Models
{
    using Input = Parsers.Models;

    public sealed class Heuristic
    {
        #region Public fields
        public HeuristicLayer Layer { get; set; }
        public int PositionNumber { get; set; }
        public HeuristicAntecedentPart[] Antecedent { get; private set; }
        public HeuristicConsequentPart Consequent { get; private set; }
        public int FreshnessStatus { get; set; }
        public bool IsAction { get; private set; }

        public Site CreatedBy { get; set; }

        public string Id
        {
            get
            {
                return $"HS{Layer.Set.PositionNumber}_L{Layer.PositionNumber}_H{PositionNumber}";
            }
        }
        #endregion

        #region Private fields
        
        #endregion

        #region Constructors
        private Heuristic() { }
        #endregion


        #region Public methods
        public bool IsMatch(Dictionary<string, dynamic> variables)
        {
            return Antecedent.All(a=>a.IsMatch(variables[a.Param]));
        }

        public void Apply(Actor actor)
        {
            actor[Consequent.Param] = Consequent.Value;
        }
        #endregion

        #region Factory methods
        internal static Heuristic Create(HeuristicAntecedentPart[] antecedent, HeuristicConsequentPart consequent)
        {
            Heuristic newHeuristic = new Heuristic();

            newHeuristic.Antecedent = antecedent;
            newHeuristic.Consequent = consequent;
            newHeuristic.FreshnessStatus = 0;
            newHeuristic.IsAction = true;

            return newHeuristic;
        }
        #endregion
    }
}
