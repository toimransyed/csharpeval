using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr.Runtime;
using ExpressionEvaluator;

namespace ExpressionEvaluator.Parser
{
    public partial class ExprEvalParser
    {
        private CompilerState compilerState = new CompilerState();

        public Expression Scope { get; set; }
        public bool IsCall { get; set; }

        public TypeRegistry TypeRegistry { get; set; }
        
        public override void ReportError(RecognitionException e)
        {
            base.ReportError(e);
            string message;
            if (e.GetType() == typeof (MismatchedTokenException))
            {
                var ex = (MismatchedTokenException) e;
                message = string.Format("Mismatched token '{0}', expected {1}", e.Token.Text, ex.Expecting);
            }
            else
            {
                message = string.Format("Error parsing token '{0}", e.Token.Text);
            }
            
            throw new ExpressionParseException(message, input);

            Console.WriteLine("Error in parser at line " + e.Line + ":" + e.CharPositionInLine);
        }

        private Expression GetIdentifier(string identifier)
        {
            ParameterExpression parameter;

            if (ParameterList.TryGetValue(identifier, out parameter))
            {
                return parameter;
            }

            object result = null;
            
            if (TypeRegistry.TryGetValue(identifier, out result))
            {
                return Expression.Constant(result);
            }
            
            return null;

            // throw new UnknownIdentifierException(identifier);
        }

        public Type GetType(string type)
        {
            object _type;

            if (TypeRegistry.TryGetValue(type, out _type))
            {
                return (Type)_type;
            }
            return Type.GetType(type);
        }

        private ParameterList ParameterList = new ParameterList();

    }
}
