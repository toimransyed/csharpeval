using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using ExpressionEvaluator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    public class Page
    {
        public bool HasSignificantScore { get; set; }
    }

    [TestClass]
    public class DynamicsPerformanceTest
    {
        [TestMethod]
        public void DynamicsAssignmentTest()
        {
            var expr = "settings.showAsteriskMessage = true;";
            expr += "settings.showStatisticallySignificantExplanation = page.HasSignificantScore;";
            //expr += "rowHeightNum = helper.getRowHeight(data);";
            expr += "rowHeight = rowHeightNum.ToString() + 'px';";
            expr += "barHeight = (rowHeightNum - 3).ToString() + 'px';";
            expr += "showPaging = count > 1;";
            dynamic scope = new ExpandoObject();
            scope.rowHeightNum = 10;
            scope.count = 2;
            scope.page = new Page();
            scope.settings = new ExpandoObject();
            var p = new CompiledExpression { StringToParse = expr };
            p.ExpressionType = CompiledExpressionType.StatementList;
            var f = p.ScopeCompile();
            f(scope);
            Assert.AreEqual(true, scope.settings.showAsteriskMessage);
            Assert.AreEqual("10px", scope.rowHeight);
            Assert.AreEqual("7px", scope.barHeight);
            Assert.AreEqual(true, scope.showPaging);
        }

        [TestMethod]
        public void DynamicsTest()
        {
            dynamic scope = new ExpandoObject();
            var fc = new FunctionCache();

            scope.Property1 = 5;
            scope.Property2 = 6;
            var expression = "Property1 + Property2";

            var st = new Stopwatch();
            st.Start();
            for (var x = 0; x < 1000000; x++)
            {
                var fn = fc.GetCachedFunction(expression);
                fn(scope);
            }
            st.Stop();
            Debug.WriteLine("{0}", st.ElapsedMilliseconds);
        }

        [TestMethod]
        public void GenericTest()
        {
            var scope = new Scope();
            var fc = new StaticFunctionCache<Scope>();

            scope.Property1 = 5;
            scope.Property2 = 6;
            var expression = "Property1 + Property2";

            var st = new Stopwatch();
            st.Start();
            for (var x = 0; x < 1000000; x++)
            {
                var fn = fc.GetCachedFunction(expression);
                fn(scope);
            }
            st.Stop();
            Debug.WriteLine("{0}", st.ElapsedMilliseconds);
        }

        [TestMethod]
        public void ObjectTest()
        {
            var scope = new Scope();
            var fc = new ObjectFunctionCache();

            scope.Property1 = 5;
            scope.Property2 = 6;
            var expression = "Property1 + Property2";

            var st = new Stopwatch();
            st.Start();
            for (var x = 0; x < 1000000; x++)
            {
                var fn = fc.GetCachedFunction(expression);
                fn(scope);
            }
            st.Stop();
            Debug.WriteLine("{0}", st.ElapsedMilliseconds);
        }



    }

    public class Scope
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
    }

    public class FunctionCache 
    {
        private readonly Dictionary<string, Func<dynamic, object>> _functionRegistry;

        public FunctionCache()
        {
            _functionRegistry = new Dictionary<string, Func<dynamic, object>>();
        }

        public int CacheHits { get; private set; }
        public int CacheMisses { get; private set; }

        public Func<dynamic, object> GetCachedFunction(string expression)
        {
            Func<dynamic, object> f;
            if (!_functionRegistry.TryGetValue(expression, out f))
            {
                CacheMisses++;
                var p = new CompiledExpression { StringToParse = expression };

                f = p.ScopeCompile();
                _functionRegistry.Add(expression, f);
            }
            else
            {
                CacheHits++;
            }
            return f;
        }

    }


    public class ObjectFunctionCache
    {
        private readonly Dictionary<string, Func<object, object>> _functionRegistry;

        public ObjectFunctionCache()
        {
            _functionRegistry = new Dictionary<string, Func<object, object>>();
        }

        public int CacheHits { get; private set; }
        public int CacheMisses { get; private set; }

        public Func<object, object> GetCachedFunction(string expression)
        {
            Func<object, object> f;
            if (!_functionRegistry.TryGetValue(expression, out f))
            {
                CacheMisses++;
                var p = new CompiledExpression { StringToParse = expression };

                f = p.ScopeCompile();
                _functionRegistry.Add(expression, f);
            }
            else
            {
                CacheHits++;
            }
            return f;
        }

    }


    public class StaticFunctionCache<T>
    {
        private readonly Dictionary<string, Func<T, object>> _functionRegistry;

        public StaticFunctionCache()
        {
            _functionRegistry = new Dictionary<string, Func<T, object>>();
        }

        public int CacheHits { get; private set; }
        public int CacheMisses { get; private set; }

        public Func<T, object> GetCachedFunction(string expression)
        {
            Func<T, object> f;
            if (!_functionRegistry.TryGetValue(expression, out f))
            {
                CacheMisses++;
                var p = new CompiledExpression { StringToParse = expression };

                f = p.ScopeCompile<T>();
                _functionRegistry.Add(expression, f);
            }
            else
            {
                CacheHits++;
            }
            return f;
        }

    }

}
