using System;
using System.Linq.Expressions;
using Antlr.Runtime;

namespace ExpressionEvaluator
{
    public partial class ExprEvalLexer
    {
        public override void ReportError(RecognitionException e)
        {
            base.ReportError(e);
            Console.WriteLine("Error in lexer at line " + e.Line + ":" + e.CharPositionInLine);
        }

    }


}
