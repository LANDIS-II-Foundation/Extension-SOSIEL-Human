using System;
using System.Linq.Expressions;

namespace SocialHuman.Builders
{
    using Models;

    class AntecedentBuilder
    {
        static Func<Expression, Expression, BinaryExpression> GetCondition(string inequalitySign)
        {
            switch (inequalitySign)
            {
                case ">":
                    return Expression.GreaterThan;
                case ">=":
                    return Expression.GreaterThanOrEqual;
                case "<":
                    return Expression.LessThan;
                case "<=":
                    return Expression.LessThanOrEqual;
                case "=":
                    return Expression.Equal;

                default:
                    throw new ArgumentException("Unsupported antecedent condition");
            }
        }

        public static Func<dynamic, bool> Build(HeuristicAntecedentPart antecedentPart)
        {
            Type paramType = antecedentPart.Const.GetType();

            // Create a parameter to use for both of the expression bodies.
            ParameterExpression param = Expression.Parameter(paramType, "x");

            // Manually build the expression tree for antecedent
            ConstantExpression antecedentConst = Expression.Constant(antecedentPart.Const, paramType);
            Func<Expression, Expression, BinaryExpression> condition = GetCondition(antecedentPart.Sign);
            BinaryExpression expression = condition(param, antecedentConst);
            Func<dynamic, bool> lambda =
                Expression.Lambda<Func<dynamic, bool>>(
                    expression,
                    new ParameterExpression[] { param }).Compile();

            return lambda;
        }

    }
}
