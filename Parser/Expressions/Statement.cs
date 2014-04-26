using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser.Expressions
{
    public class Statement
    {
        public Expression Expression { get; set; }
        public List<Expression> Initializers { get; set; } 
    }
}