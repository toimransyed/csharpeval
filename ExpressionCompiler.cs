using System;
using System.Linq.Expressions;
using ExpressionEvaluator.Parser;

namespace ExpressionEvaluator
{
    public enum CompiledExpressionType
    {
        Expression = 0,
        Statement = 1,
        StatementList = 2
    }

    public abstract class ExpressionCompiler
    {
        public Expression Expression = null;
        public CompiledExpressionType ExpressionType { get; set; }

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
            Parser.ExpressionType = ExpressionType;
            return Expression = Parser.Parse(scopeParam, isCall);
        }

        protected abstract void ClearCompiledMethod();

        protected void Parse()
        {
            BuildTree(null, false);
        }

        protected Expression WrapExpression(Expression source, bool castToObject)
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