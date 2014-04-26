using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser.Expressions
{
    public class Variable
    {
        public string Identifier { get; set; }
        public Expression Initializer { get; set; }
    }
}