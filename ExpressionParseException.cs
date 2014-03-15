using System;

namespace ExpressionEvaluator
{
    [Serializable]
    public class ExpressionParseException: Exception
    {
        public ExpressionParseException(string message)
            : base(message)
        {
            
        }
    }
}