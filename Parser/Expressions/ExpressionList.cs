using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser.Expressions
{
    public class ExpressionList : PrimaryExpressionPart
    {
        public List<Expression> Values { get; set; }
    }
}