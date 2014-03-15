using System;
using System.Collections.Generic;
using System.Linq;
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

    public class ParameterList
    {
        private List<ParameterExpression> _parameters = new List<ParameterExpression>();

        public void Add(List<ParameterExpression> list)
        {
            foreach (var parameterExpression in list)
            {
                ParameterExpression p;
                if (!ParameterLookup.TryGetValue(parameterExpression.Name, out p))
                {
                    _parameters.Add(parameterExpression);
                }
                else
                {
                    throw new ExpressionParseException("Parameter conflicts with existing parameter");
                }
            }
        }

        private Dictionary<string, ParameterExpression> ParameterLookup { get { return _parameters.ToDictionary(expression => expression.Name); } }

        public bool TryGetValue(string name, out ParameterExpression parameter)
        {
            return ParameterLookup.TryGetValue(name, out parameter);
        }

        public void Remove(List<ParameterExpression> list)
        {
            foreach (var parameterExpression in list)
            {
                ParameterExpression p;
                if (ParameterLookup.TryGetValue(parameterExpression.Name, out p))
                {
                    _parameters.Remove(parameterExpression);
                }
                else
                {
                    throw new ExpressionParseException("Parameter not found while removing???");
                }
            }
        }

    }
}
