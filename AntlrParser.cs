using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Antlr.Runtime;

namespace ExpressionEvaluator
{
    public class AntlrParser
    {
        public string ExpressionString { get; set; }
        public Expression Expression { get; set; }

        public Dictionary<string, object> TypeRegistry { get; set; }

        public AntlrParser()
        {
            TypeRegistry = new Dictionary<string, object>();
        }

        public AntlrParser(string expression)
        {
            ExpressionString = expression;
            TypeRegistry = new Dictionary<string, object>();
        }

        public void RegisterType(string identifier, object value)
        {
            TypeRegistry.Add(identifier, value);
        }

        public Expression Parse(Expression scope, bool isCall = false)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(ExpressionString));
            var input = new ANTLRInputStream(ms);
            var lexer = new ExprEvalLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ExprEvalParser(tokens) { TypeRegistry = TypeRegistry, Scope = scope, IsCall = isCall };
            do
            {
                Expression = parser.expression();
            } while (parser.input.Index == 0);

            return Expression;
        }

        public T Eval<T>()
        {
            Parse(null, false);
            var funcexp = Expression.Lambda<Func<T>>(Expression, null).Compile();
            return funcexp();
        }

        public object Global { get; set; }
    }
}
