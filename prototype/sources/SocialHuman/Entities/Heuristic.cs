using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Entities
{
    using Builders;
    using Models;

    public sealed class Heuristic
    {
        #region Public fields
        public HeuristicLayer Layer { get; set; }
        public int PositionNumber { get; set; }
        public string AntecedentInequalitySign { get; private set; }
        public AnticipatedInfluence[] AnticipatedInfluences { get; set; }
        public int FreshnessStatus { get; set; }
        public bool IsAction { get; private set; }
        public double ConsequentValue { get; private set; }
        public double AncetedentConst { get; private set; }

        public string Id
        {
            get
            {
                return $"HS{Layer.Set.PositionNumber}_L{Layer.PositionNumber}_H{PositionNumber}";
            }
        }
        #endregion

        #region Private fields
        private Func<double, bool> antecedent;
        #endregion

        #region Constructors
        private Heuristic() { }
        #endregion


        #region Public methods
        public bool IsMatch(double goalValue)
        {
            return antecedent(goalValue);
        }

        public AnticipatedInfluence ForGoal(ActorGoal goal)
        {
            return AnticipatedInfluences.Single(ai => ai.ActorGoal == goal);
        }
        #endregion

        #region Factory methods
        private static Heuristic Create(HeuristicParameters parameters)
        {
            Heuristic heuristic = new Heuristic
            {
                AncetedentConst = parameters.AntecedentConst,
                ConsequentValue = parameters.ConsequentValue,
                AntecedentInequalitySign = parameters.AntecedentInequalitySign,
                FreshnessStatus = parameters.FreshnessStatus,
                IsAction = parameters.IsAction,
            };

            heuristic.antecedent = AntecedentBuilder.Build(parameters);

            return heuristic;
        }
        public static Heuristic Create(HeuristicParameters parameters, IEnumerable<ActorGoal> actorGoals)
        {
            Heuristic heuristic = Create(parameters);

            heuristic.AnticipatedInfluences = parameters.AnticipatedInfluence.Zip(actorGoals, (av, ag) => new AnticipatedInfluence(ag, av)).ToArray();

            return heuristic;
        }
        public static Heuristic Create(HeuristicParameters parameters, IEnumerable<ActorGoalState> goalStates)
        {
            Heuristic heuristic = Create(parameters);

            heuristic.AnticipatedInfluences = goalStates.Select(gs => new AnticipatedInfluence(gs.Goal, gs.AnticipatedInfluenceValue)).ToArray();

            return heuristic;
        }
        #endregion
    }
}
