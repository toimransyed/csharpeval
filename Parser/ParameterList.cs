using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser
{
    public class ParameterList
    {
        private readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();

        public void Add(ParameterExpression parameter)
        {
            ParameterExpression p;
            if (!ParameterLookup.TryGetValue(parameter.Name, out p))
            {
                _parameters.Add(parameter);
            }
            else
            {
                throw new Exception(string.Format("A local variable named '{0}' cannot be declared in this scope because it would give a different meaning to '{0}', which is already used in a 'parent or current' scope to denote something else", parameter.Name));
            }

        }

        public void Add(List<ParameterExpression> list)
        {
            foreach (var parameterExpression in list)
            {
                Add(parameterExpression);
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
            }
        }

    }
}