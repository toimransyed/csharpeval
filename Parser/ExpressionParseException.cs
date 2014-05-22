using System;
using Antlr.Runtime;

namespace ExpressionEvaluator.Parser
{
    [Serializable]
    public class ExpressionParseException : Exception
    {
        private ITokenStream _tokenStream;

        public ExpressionParseException(string message, ITokenStream tokenStream)
            : base(string.Format("{0} at line {1} char {2}", message, tokenStream.LT(-1).Line, tokenStream.LT(-1).CharPositionInLine))
        {
            this._tokenStream = tokenStream;
        }
    }
}