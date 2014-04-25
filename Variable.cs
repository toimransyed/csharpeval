using System.Linq.Expressions;

namespace ExpressionEvaluator
{
    public class Variable
    {
        public string Identifier { get; set; }
        public Expression Initializer { get; set; }
    }
}