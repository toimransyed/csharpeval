using System;
using System.Linq.Expressions;

namespace ExpressionEvaluator
{
    public class CompiledExpression<T> : ExpressionCompiler
    {
        private Func<T> _compiledMethod = null;

        public CompiledExpression()
        {
            Parser = new Parser();
            Parser.TypeRegistry = TypeRegistry;

        }

        public CompiledExpression(string expression)
        {
            Parser = new Parser(expression);
            Parser.TypeRegistry = TypeRegistry;
        }

        public Func<T> Compile()
        {
            if (Expression == null) Expression = BuildTree();
            return Expression.Lambda<Func<T>>(Expression).Compile();
        }

        public Func<dynamic, T> ScopeCompile()
        {
            var scopeParam = Expression.Parameter(typeof(object), "scope");
            if (Expression == null) Expression = BuildTree(scopeParam);
            return Expression.Lambda<Func<dynamic, T>>(Expression.Convert(Expression, typeof(object)), new ParameterExpression[] { scopeParam }).Compile();
        }

        protected override void ClearCompiledMethod()
        {
            _compiledMethod = null;
        }

        public T Eval()
        {
            if (_compiledMethod == null) _compiledMethod = Compile();
            return _compiledMethod();
        }

        public object Global
        {
            set
            {
                Parser.Global = value;
            }
        }
 
    }

    public class CompiledExpression : ExpressionCompiler
    {
        private Func<object> _compiledMethod = null;

        public CompiledExpression()
        {
            Parser = new Parser();
            Parser.TypeRegistry = TypeRegistry;

        }

        public CompiledExpression(string expression)
        {
            Parser = new Parser(expression); 
            Parser.TypeRegistry = TypeRegistry;
        }

        public Func<object> Compile()
        {
            if (Expression == null) Expression = BuildTree();
            return Expression.Lambda<Func<object>>(Expression.Convert(Expression, typeof(object))).Compile();
        }


        public Func<dynamic, object> ScopeCompile()
        {
            var scopeParam = Expression.Parameter(typeof(object), "scope");
            if (Expression == null) Expression = BuildTree(scopeParam);
            return Expression.Lambda<Func<dynamic, object>>(Expression.Convert(Expression, typeof(object)), new ParameterExpression[] { scopeParam }).Compile();
        }

        protected override void ClearCompiledMethod()
        {
            _compiledMethod = null;
        }

        public object Eval()
        {
            if (_compiledMethod == null) _compiledMethod = Compile();
            return _compiledMethod();
        }

        public object Global
        {
            set
            {
                Parser.Global = value;
            }
        }

        public object Global
        {
            set
            {
                Parser.Global = value;
            }
        }
    }
}
