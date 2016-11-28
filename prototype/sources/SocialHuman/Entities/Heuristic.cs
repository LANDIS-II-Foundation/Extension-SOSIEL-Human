using System;

namespace SocialHuman.Entities
{
    using Builders;
    using Models;

    public sealed class Heuristic
    {
        public HeuristicLayer Layer { get; set; }

        public int PositionNumber { get; set; }

        public string AntecedentInequalitySign { get; private set; }
        public double AnticipatedInfluence { get; set; }
        public int FreshnessStatus { get; set; } 
        public bool IsAction { get; private set; }
        


        public double ConsequentValue { get; private set; }

        public string Id
        {
            get
            {
                return $"HS{Layer.Set.PositionNumber}_L{Layer.PositionNumber}_H{PositionNumber}";
            }
        }

        private Func<double, bool> antecedent;

        public bool IsMatch(double goalValue)
        {
            return antecedent(goalValue);
        }

        private Heuristic() { }

        public static Heuristic Create(HeuristicParameters parameters)
        {
            Heuristic heuristic = new Heuristic
            {
                AnticipatedInfluence = parameters.AnticipatedInfluence,
                ConsequentValue = parameters.ConsequentValue,
                AntecedentInequalitySign = parameters.AntecedentInequalitySign,
                FreshnessStatus = parameters.FreshnessStatus,
                IsAction = parameters.IsAction,
            };

            heuristic.antecedent = AntecedentBuilder.Build(parameters);

            return heuristic;
        }
    }
}
