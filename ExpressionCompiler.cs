using System;
using System.Collections.Generic;
using System.Linq;
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
        public Expression Expression { get; set; }
        public CompiledExpressionType ExpressionType { get; set; }
        public LambdaExpression LambdaExpression { get; set; }

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

        public T Compile<T>(params string[] parameters)
        {
            var f = typeof (T);
            var argTypes = f.GetGenericArguments();
            var argParams = parameters.Select((t, i) => Expression.Parameter(argTypes[i], t)).ToList();
            Parser.ExternalParameters = argParams;
            Expression = BuildTree();
            return Expression.Lambda<T>(Expression, argParams).Compile();
        }

        public void ScopeParse()
        {
            var scopeParam = Expression.Parameter(typeof(object), "scope");
            Expression = BuildTree(scopeParam);
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