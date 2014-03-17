using System;
using System.Linq.Expressions;

namespace ExpressionEvaluator
{
    public abstract class ExpressionCompiler
    {
        protected Expression Expression = null;
        protected AntlrParser Parser = null;
        public TypeRegistry TypeRegistry { get; set; }

        protected string Pstr = null;

        public string StringToParse
        {
            get { return Parser.ExpressionString; }
            set
            {
                Parser.ExpressionString = value;
                Expression = null;
                ClearCompiledMethod();
            }
        }

        protected Expression BuildTree(Expression scopeParam = null, bool isCall = false)
        {
            Parser.TypeRegistry = TypeRegistry;
            return Expression = Parser.Parse(scopeParam, isCall);
        }

        protected abstract void ClearCompiledMethod();

        protected void Parse()
        {
            BuildTree(null, false);
        }

        protected Expression WrapExpression(Expression source, bool castToObject = true)
        {
            if (source.Type == typeof(void))
            {
                return WrapToNull(source);
            }
            return castToObject ? Expression.Convert(source, typeof(object)) : Expression;
        }

        protected Expression WrapToVoid(Expression source)
        {
            return Expression.Block(source, Expression.Empty());
        }

        protected Expression WrapToNull(Expression source)
        {
            return Expression.Block(source, Expression.Constant(null));
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}