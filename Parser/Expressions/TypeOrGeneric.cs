using System;
using System.Collections.Generic;

namespace ExpressionEvaluator.Parser.Expressions
{
    public class TypeOrGeneric
    {
        public string Identifier { get; set; }
        public List<Type> TypeArgs { get; set; }
    }
}