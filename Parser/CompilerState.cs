using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser
{
    internal class CompilerState
    {
        public LabelTarget ReturnTarget { get; set; }
        public Stack<LabelTarget> BreakContext { get; private set; }
        public Stack<LabelTarget> ContinueContext { get; private set; }
        public LabelTarget CurrentBreak { get; private set; }
        public LabelTarget CurrentContinue { get; private set; }

        public CompilerState()
        {
            BreakContext = new Stack<LabelTarget>();
            ContinueContext = new Stack<LabelTarget>();
        }

        public LabelTarget PushContinue()
        {
            CurrentContinue = Expression.Label();
            ContinueContext.Push(CurrentContinue);
            return CurrentContinue;
        }

        public LabelTarget PushBreak()
        {
            CurrentBreak = Expression.Label();
            BreakContext.Push(CurrentBreak);
            return CurrentBreak;
        }

        public void PopBreak()
        {
            CurrentBreak = BreakContext.Pop();
        }

        public void PopContinue()
        {
            CurrentContinue = ContinueContext.Pop();
        }

        public Expression Break()
        {
            if (BreakContext.Count == 0)
            {
                throw new Exception("No enclosing loop out of which to break or continue");
            }
            return Expression.Break(BreakContext.Peek());
        }

        public Expression Continue()
        {
            if (ContinueContext.Count == 0)
            {
                throw new Exception("No enclosing loop out of which to break or continue");
            }
            return Expression.Continue(ContinueContext.Peek());
        }

    }
}