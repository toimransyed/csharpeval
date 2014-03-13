using System;

namespace ExpressionEvaluator
{
    [Serializable]
    public class InvalidTokenException: Exception
    {
        public InvalidTokenException(string message) : base(message)
        {
            
        }
    }

    public class UnknownIdentifierException : InvalidTokenException
    {
        public UnknownIdentifierException(string message) : base(message)
        {
        }
    }

    public class InvalidLiteralException : InvalidTokenException
    {
        public InvalidLiteralException(string message) : base(message)
        {
        }
    }
}