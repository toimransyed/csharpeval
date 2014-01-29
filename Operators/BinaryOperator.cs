using System;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Operators
{
    internal class BinaryOperator : Operator<Func<Expression, Expression, Expression>>
    {
        public BinaryOperator(string value, int precedence, bool leftassoc,
                              Func<Expression, Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
            Arguments = 2;
        }
    }

    internal class TernaryOperator : Operator<Func<Expression, Expression, Expression, Expression>>
    {
        public TernaryOperator(string value, int precedence, bool leftassoc,
                              Func<Expression, Expression, Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
            Arguments = 3;
        }
    }

}