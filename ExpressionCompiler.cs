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

        public T Compile<T>(params string[] parameters)
        {
            var f = typeof (T);
            var argTypes = f.GetGenericArguments();
            if (argTypes.Length - parameters.Length != 1)
            {
                throw new Exception("Type arguments must be 1 more than the number of parameters");
            }
            var argParams = parameters.Select((t, i) => Expression.Parameter(argTypes[i], t)).ToList();
            Parser.ExternalParameters = argParams;
            if (Expression == null) Expression = WrapExpression(BuildTree(), true);
            return Expression.Lambda<T>(Expression, argParams).Compile();
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