using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr.Runtime;

namespace ExpressionEvaluator
{
    public partial class ExprEvalParser
    {
        public Expression Scope { get; set; }
        public bool IsCall { get; set; }

        public TypeRegistry TypeRegistry { get; set; }

        public override void ReportError(RecognitionException e)
        {
            base.ReportError(e);
            Console.WriteLine("Error in parser at line " + e.Line + ":" + e.CharPositionInLine);
        }

        private Expression GetIdentifier(string identifier)
        {
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

    }
}
