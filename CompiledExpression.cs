using System;
using System.Linq.Expressions;

namespace ExpressionEvaluator
{

    /// <summary>
    /// Creates compiled expressions with return values that are of type T
    /// </summary>
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

        public Func<object, T> ScopeCompile()
        {
            var scopeParam = Expression.Parameter(typeof(object), "scope");
            if (Expression == null) Expression = BuildTree(scopeParam);
            return Expression.Lambda<Func<dynamic, T>>(Expression.Convert(Expression, typeof(object)), new ParameterExpression[] { scopeParam }).Compile();
        }

        public Func<U, T> ScopeCompile<U>()
        {
            var scopeParam = Expression.Parameter(typeof(U), "scope");
            if (Expression == null) Expression = BuildTree(scopeParam);
            return Expression.Lambda<Func<U, T>>(Expression.Convert(Expression, typeof(object)), new ParameterExpression[] { scopeParam }).Compile();
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

    /// <summary>
    /// Creates compiled expressions with return values that are cast to type Object 
    /// </summary>
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

        /// <summary>
        /// Compiles the expression to a function that returns an object
        /// </summary>
        /// <returns></returns>
        public Func<object> Compile()
        {
            if (Expression == null) Expression = BuildTree();
            return Expression.Lambda<Func<object>>(Expression.Convert(Expression, typeof(object))).Compile();
        }

        /// <summary>
        /// Compiles the expression to a function that takes an object as a parameter and returns an object
        /// </summary>
        /// <returns></returns>
        public Func<object, object> ScopeCompile()
        {
            var scopeParam = Expression.Parameter(typeof(object), "scope");
            if (Expression == null) Expression = BuildTree(scopeParam);
            return Expression.Lambda<Func<dynamic, object>>(Expression.Convert(Expression, typeof(object)), new ParameterExpression[] { scopeParam }).Compile();
        }

        /// <summary>
        /// Compiles the expression to a function that takes an typed object as a parameter and returns an object
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public Func<U, object> ScopeCompile<U>()
        {
            var scopeParam = Expression.Parameter(typeof(U), "scope");
            if (Expression == null) Expression = BuildTree(scopeParam);
            return Expression.Lambda<Func<U, object>>(Expression.Convert(Expression, typeof(object)), new ParameterExpression[] { scopeParam }).Compile();
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

    }
}
