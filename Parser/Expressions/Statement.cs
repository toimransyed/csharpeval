using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser.Expressions
{
    public abstract class MultiStatement
    {

    }

    public class StatementList : MultiStatement
    {
        public StatementList()
        {
            Expressions = new List<Expression>();
        }

        public void Add(Expression expression)
        {
            Expressions.Add(expression);
        }
        public List<Expression> Expressions { get; private set; }
    }

    public class LocalVariableDeclaration : MultiStatement
    {
        public List<ParameterExpression> Variables { get; set; }
        public List<Expression> Initializers { get; set; }
    }

    public class Statement
    {
        public Expression Expression { get; set; }
        public List<Expression> Initializers { get; set; }
    }
}