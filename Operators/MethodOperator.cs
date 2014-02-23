using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Operators
{
    internal class MethodOperator : Operator<Func<bool, Expression, string, List<Expression>, List<string>, Expression>>
    {
        public MethodOperator(string value, int precedence, bool leftassoc,
                              Func<bool, Expression, string, List<Expression>, List<string>, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
        }
    }

    internal class TernarySeparatorOperator : Operator<Func<Expression, Expression>>
    {
        public TernarySeparatorOperator(string value, int precedence, bool leftassoc,
                              Func<Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
        }
    }
}