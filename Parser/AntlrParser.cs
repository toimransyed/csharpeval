using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Antlr.Runtime;

namespace ExpressionEvaluator.Parser
{
    public class AntlrParser
    {
        public string ExpressionString { get; set; }
        public Expression Expression { get; set; }

        public TypeRegistry TypeRegistry { get; set; }

        public AntlrParser()
        {
        }

        public AntlrParser(string expression)
        {
            ExpressionString = expression;
        }

        public Expression Parse(Expression scope, bool isCall = false)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(ExpressionString));
            var input = new ANTLRInputStream(ms);
            var lexer = new ExprEvalLexer(input);
            var tokens = new TokenRewriteStream(lexer);
            if (TypeRegistry == null) TypeRegistry = new TypeRegistry();
            var parser = new ExprEvalParser(tokens) { TypeRegistry = TypeRegistry, Scope = scope, IsCall = isCall };
            switch (ExpressionType)
            {
                case CompiledExpressionType.Expression:
                    Expression = parser.expression();
                    break;
                case CompiledExpressionType.Statement:
                    var statement = parser.statement();
                    if (statement != null)
                    {
                        Expression = statement.Expression;
                    }
                    break;
                case CompiledExpressionType.StatementList:
                    var statements = parser.statement_list();

                    var variables = statements.Where(x => x.Expression.NodeType == System.Linq.Expressions.ExpressionType.RuntimeVariables).ToList();
                    var parameters = variables.Select(x => x.Expression).Cast<RuntimeVariablesExpression>().SelectMany(x => x.Variables).ToList();
                    var initializers = variables.SelectMany(x => x.Initializers).ToList();
                    var expressions = new List<Expression>();
                    if (initializers.Any())
                    {
                        expressions.AddRange(initializers);
                    }
                    expressions.AddRange(statements.Where(x => x.Expression.NodeType != System.Linq.Expressions.ExpressionType.RuntimeVariables).Select(x => x.Expression).ToList());


                    if (parameters.Any())
                    {
                        Expression = Expression.Block(parameters, expressions);
                    }
                    else
                    {
                        Expression = Expression.Block(expressions);
                    }

                    break;
            }
            return Expression;
        }

        public object Global { get; set; }

        public CompiledExpressionType ExpressionType { get; set; }
    }
}
