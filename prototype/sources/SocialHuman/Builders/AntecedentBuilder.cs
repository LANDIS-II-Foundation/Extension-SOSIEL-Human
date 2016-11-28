using System;
using System.Linq.Expressions;

namespace SocialHuman.Builders
{
    using Models;

    public class AntecedentBuilder
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

        public static Func<double, bool> Build(AntecedentParameters parameters)
        {
            // Create a parameter to use for both of the expression bodies.
            ParameterExpression intParam = Expression.Parameter(typeof(double), "x");

            // Manually build the expression tree for antecedent
            ConstantExpression antecedentConst = Expression.Constant(parameters.AntecedentConst, typeof(double));
            Func<Expression, Expression, BinaryExpression> condition = GetCondition(parameters.AntecedentInequalitySign);
            BinaryExpression expression = condition(intParam, antecedentConst);
            Func<double, bool> lambda =
                Expression.Lambda<Func<double, bool>>(
                    expression,
                    new ParameterExpression[] { intParam }).Compile();

            return lambda;
        }

    }
}
