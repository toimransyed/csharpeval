using System;
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
                    Expression = statements.ToBlock();
                    break;
            }
            return Expression;
        }

        public object Global { get; set; }

        public CompiledExpressionType ExpressionType { get; set; }

        public Type ReturnType { get; set; }
    }
}
