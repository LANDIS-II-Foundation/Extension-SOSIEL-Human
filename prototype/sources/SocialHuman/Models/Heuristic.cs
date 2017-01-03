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

        public HeuristicAntecedentPart[] Antecedent { get; set; }

        public HeuristicConsequentPart Consequent { get; set; }

        public int FreshnessStatus { get; set; }

        public bool IsAction { get; private set; }
               
        public int RequiredParticipants { get; private set; }

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
        public bool IsMatch(Func<string, dynamic> func)
        {
            return Antecedent.All(a=>a.IsMatch(func(a.Param)));
        }

        public int CountRightCondition(Func<string, dynamic> func)
        {
            return Antecedent.Count(a => a.IsMatch(func(a.Param)));
        }

        public double GetConsequentValue()
        {
            double value = Consequent.Value;

            //if (IsCollectiveAction)
            //{
            //    value /= RequiredParticipants;
            //}

            return value;
        }

        public void Apply(Actor actor)
        {
            actor[Consequent.Param] = GetConsequentValue();
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
