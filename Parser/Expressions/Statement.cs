using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser.Expressions
{
    public class Statement
    {
        public Expression Expression { get; set; }
    }

    public class MultiStatement : Statement
    {
    }



    public class DeclarationStatement : MultiStatement
    {
    }

    public class StatementList : MultiStatement
    {
        public StatementList()
        {
            Statements = new List<Statement>();
            Variables = new List<LocalVariableDeclaration>();
        }

        public void Add(Statement statement)
        {
            if (statement.GetType() == typeof(LocalVariableDeclaration))
            {
                Variables.Add((LocalVariableDeclaration)statement);
            }
            else
            {
                Statements.Add(statement);
            }

        }

        public void Add(Expression expression)
        {
            Statements.Add(new Statement() { Expression = expression });
        }

        public List<LocalVariableDeclaration> Variables { get; set; }

        public List<Statement> Statements { get; private set; }

        public IEnumerable<Expression> Expressions { get { return Statements.Select(x => x.Expression); } }

        public BlockExpression ToBlock()
        {
            var expressions = new List<Expression>();
            IList<ParameterExpression> parameters = null;

            if (Variables != null && Variables.Any())
            {
                var variables = Variables;
                parameters = variables.SelectMany(x => x.Variables).ToList();
                var initializers = variables.SelectMany(x => x.Initializers).ToList();

                if (initializers.Any())
                {
                    expressions.AddRange(initializers);
                }
            }

            expressions.AddRange(Statements.Select(x => x.Expression));

            if (parameters != null && parameters.Any())
            {
                return Expression.Block(parameters, expressions);
            }

            return Expression.Block(expressions);
        }
    }

    public class LocalVariableDeclaration : DeclarationStatement
    {
        public LocalVariableDeclaration()
        {
            Variables = new List<ParameterExpression>();
            Initializers = new List<Expression>();
        }

        public List<ParameterExpression> Variables { get; set; }
        public List<Expression> Initializers { get; set; }
    }

    public class LocalConstDeclaration : DeclarationStatement
    {
        //public List<ParameterExpression> Variables { get; set; }
        //public List<Expression> Initializers { get; set; }
    }



}