using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionEvaluator.Parser;

namespace ExpressionEvaluator
{

    /// <summary>
    /// Creates compiled expressions with return values that are of type T
    /// </summary>
    public class CompiledExpression<TResult> : ExpressionCompiler
    {
        private Func<TResult> _compiledMethod = null;
        private Action _compiledAction = null;

        public CompiledExpression()
        {
            Parser = new AntlrParser();
            Parser.ReturnType = typeof(TResult);
        }

        public CompiledExpression(string expression)
        {
            Parser = new AntlrParser(expression);
            Parser.ReturnType = typeof(TResult);
        }

        public Func<TResult> Compile(bool isCall = false)
        {
            Expression = WrapExpression(BuildTree(), false);
            return Expression.Lambda<Func<TResult>>(Expression).Compile();
        }

        public Expression<T> GenerateLambda<T, TParam>(bool withScope, bool asCall)
        {
            var scopeParam = Expression.Parameter(typeof(TParam), "scope");
            var expression = withScope ? BuildTree(scopeParam, asCall) : BuildTree();
            Expression = WrapExpression(expression, false);
            return withScope ? 
                Expression.Lambda<T>(Expression, new ParameterExpression[] { scopeParam }) :
                Expression.Lambda<T>(Expression)
                ;
        }

        private T Compile<T, TParam>(bool withScope, bool asCall) 
        {
            return GenerateLambda<T, TParam>(withScope, asCall).Compile();
        }

        //    public LambdaExpression GenerateLambda()
        //    {
        //        var scopeParam = Expression.Parameter(typeof(object), "scope");
        //        Expression = WrapExpression(BuildTree(scopeParam), true);
        //        return Expression.Lambda<Func<dynamic, object>>(Expression, new ParameterExpression[] { scopeParam });
        //    }

        /// <summary>
        /// Compiles the expression to a function that returns void
        /// </summary>
        /// <returns></returns>
        public Action CompileCall()
        {
            Expression = BuildTree(null, true);
            return Expression.Lambda<Action>(Expression).Compile();
        }

        /// <summary>
        /// Compiles the expression to a function that takes an object as a parameter and returns an object
        /// </summary>
        /// <returns></returns>
        public Action<object> ScopeCompileCall()
        {
            return ScopeCompileCall<object>();
        }

        /// <summary>
        /// Compiles the expression to a function that takes an object as a parameter and returns an object
        /// </summary>s
        /// <returns></returns>
        public Action<TParam> ScopeCompileCall<TParam>()
        {
            return CompileWithScope<Action<TParam>, TParam>(true);
        }

        public Func<object, TResult> ScopeCompile()
        {
            return ScopeCompile<object>();
        }

        public Func<TParam, TResult> ScopeCompile<TParam>()
        {
            return CompileWithScope<Func<TParam, TResult>, TParam>(false);
        }

        private T CompileWithScope<T, TParam>(bool asCall)
        {
            var scopeParam = Expression.Parameter(typeof(TParam), "scope");
            Expression = BuildTree(scopeParam, asCall);
            return Expression.Lambda<T>(Expression, new ParameterExpression[] { scopeParam }).Compile();
        }

        protected override void ClearCompiledMethod()
        {
            _compiledMethod = null;
            _compiledAction = null;
        }

        public TResult Eval()
        {
            if (_compiledMethod == null) _compiledMethod = Compile();
            return _compiledMethod();
        }

        public void Call()
        {
            if (_compiledAction == null) _compiledAction = CompileCall();
            _compiledAction();
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
    public class CompiledExpression : CompiledExpression<object>
    {
        public CompiledExpression()
        {
        }

        public CompiledExpression(string expression)
            : base(expression)
        {

        }
    }

    //public class CompiledExpression : ExpressionCompiler
    //{
    //    private Func<object> _compiledMethod = null;
    //    private Action _compiledAction = null;

    //    public CompiledExpression()
    //    {
    //        Parser = new AntlrParser();
    //        Parser.ReturnType = typeof(object);
    //    }

    //    public CompiledExpression(string expression)
    //    {
    //        Parser = new AntlrParser(expression);
    //    }

    //    /// <summary>
    //    /// Compiles the expression to a function that returns an object
    //    /// </summary>
    //    /// <returns></returns>
    //    public Func<object> Compile()
    //    {
    //        Expression = WrapExpression(BuildTree(), true);
    //        return Expression.Lambda<Func<object>>(Expression).Compile();
    //    }


    //    /// <summary>
    //    /// Compiles the expression to a function that returns void
    //    /// </summary>
    //    /// <returns></returns>
    //    public Action CompileCall()
    //    {
    //        Expression = BuildTree(null, true);
    //        return Expression.Lambda<Action>(Expression).Compile();
    //    }

    //    /// <summary>
    //    /// Compiles the expression to a function that takes an object as a parameter and returns an object
    //    /// </summary>
    //    /// <returns></returns>
    //    public Func<object, object> ScopeCompile()
    //    {
    //        var scopeParam = Expression.Parameter(typeof(object), "scope");
    //        Expression = WrapExpression(BuildTree(scopeParam), true);
    //        return Expression.Lambda<Func<dynamic, object>>(Expression, new ParameterExpression[] { scopeParam }).Compile();
    //    }

    //    public LambdaExpression GenerateLambda()
    //    {
    //        var scopeParam = Expression.Parameter(typeof(object), "scope");
    //        Expression = WrapExpression(BuildTree(scopeParam), true);
    //        return Expression.Lambda<Func<dynamic, object>>(Expression, new ParameterExpression[] { scopeParam });
    //    }

    //    /// <summary>
    //    /// Compiles the expression to a function that takes an object as a parameter and returns an object
    //    /// </summary>
    //    /// <returns></returns>
    //    public Action<object> ScopeCompileCall()
    //    {
    //        var scopeParam = Expression.Parameter(typeof(object), "scope");
    //        Expression = BuildTree(scopeParam, true);
    //        return Expression.Lambda<Action<dynamic>>(Expression, new ParameterExpression[] { scopeParam }).Compile();
    //    }

    //    /// <summary>
    //    /// Compiles the expression to a function that takes an object as a parameter and returns an object
    //    /// </summary>s
    //    /// <returns></returns>
    //    public Action<TParam> ScopeCompileCall<TParam>()
    //    {
    //        var scopeParam = Expression.Parameter(typeof(TParam), "scope");
    //        Expression = WrapToVoid(BuildTree(scopeParam));
    //        return Expression.Lambda<Action<TParam>>(Expression, new ParameterExpression[] { scopeParam }).Compile();
    //    }

    //    /// <summary>
    //    /// Compiles the expression to a function that takes an typed object as a parameter and returns an object
    //    /// </summary>
    //    /// <typeparam name="U"></typeparam>
    //    /// <returns></returns>
    //    public Func<TParam, object> ScopeCompile<TParam>()
    //    {
    //        var scopeParam = Expression.Parameter(typeof(TParam), "scope");
    //        Expression = WrapExpression(BuildTree(scopeParam), true);
    //        return Expression.Lambda<Func<TParam, object>>(Expression, new ParameterExpression[] { scopeParam }).Compile();
    //    }

    //    protected override void ClearCompiledMethod()
    //    {
    //        _compiledMethod = null;
    //        _compiledAction = null;
    //    }

    //    public object Eval()
    //    {
    //        if (_compiledMethod == null) _compiledMethod = Compile();
    //        return _compiledMethod();
    //    }

    //    public void Call()
    //    {
    //        if (_compiledAction == null) _compiledAction = CompileCall();
    //        _compiledAction();
    //    }

    //    public object Global
    //    {
    //        set
    //        {
    //            Parser.Global = value;
    //        }
    //    }

    //}
}
