using System;
using System.Collections.Generic;

namespace ExpressionEvaluator.Parser
{
    internal class TypeVariable
    {
        public TypeVariable()
        {
            Bounds = new List<Type>();
            UpperBounds = new List<Type>();
            LowerBounds = new List<Type>();
        }

        public bool IsFixed { get; set; }
        public string Name { get; set; }
        public List<Type> Bounds { get; set; }
        public List<Type> UpperBounds { get; set; }
        public List<Type> LowerBounds { get; set; }
    }
}