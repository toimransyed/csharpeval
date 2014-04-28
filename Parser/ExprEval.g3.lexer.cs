using System;
using Antlr.Runtime;

namespace ExpressionEvaluator.Parser
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
