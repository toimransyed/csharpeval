using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator
{
    public class Statement
    {
        public Expression Expression { get; set; }
        public List<Expression> Initializers { get; set; } 
    }
}