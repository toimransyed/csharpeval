using System;
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ExpressionEvaluator.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressionEvaluator;
using UnitTestProject1;

namespace ExpressionEvaluator.Tests
{
    public class TestClass
    {
        public TestClass(int value)
        {

        }
    }
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest
    {
        public UnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ExpressionParseException))]
        public void ParseInvalidNumericThrowsException()
        {
            var str = "2.55DX";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
        }

        [TestMethod]
        public void UnderscoreVariables()
        {
            var str = "1 | VARIABLE_NAME | _VARNAME";
            var t = new TypeRegistry();
            t.RegisterSymbol("VARIABLE_NAME", 16);
            t.RegisterSymbol("_VARNAME", 32);
            var c = new CompiledExpression(str) { TypeRegistry = t };
            var ret = c.Eval();
        }

        [TestMethod]
        public void New()
        {
            var str = "new TestClass(123)";
            var t = new TypeRegistry();
            t.RegisterType("TestClass", typeof(TestClass));
            var c = new CompiledExpression<TestClass>(str) { TypeRegistry = t };
            var ret = c.Eval();
        }

        [TestMethod]
        public void ParseDSuffixReturnsDouble()
        {
            var str = "2.5D";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            Assert.IsTrue(ret.GetType() == typeof(System.Double));
            Assert.IsTrue(Convert.ToDouble(ret) == 2.5D);
        }

        [TestMethod]
        public void ImplicitNumericCasting()
        {
            var str = "2.5D + 1";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            Assert.IsTrue(ret.GetType() == typeof(System.Double));
            Assert.IsTrue(Convert.ToDouble(ret) == 3.5D);
        }

        public class Container
        {
            public int x { get; set; }
        }

        [TestMethod]
        public void Assignment()
        {
            var str = "c.x = 1";
            var c = new CompiledExpression(str) { TypeRegistry = new TypeRegistry() };
            var cont = new Container();
            c.TypeRegistry.RegisterSymbol("c", cont);
            c.TypeRegistry.RegisterType("p", typeof(Math));
            var ret = c.Eval();
            Assert.AreEqual(ret, 1);
            Assert.AreEqual(ret, cont.x);
        }


        [TestMethod]
        public void TernaryOperator()
        {
            var str = "3 == 2 ? 4 : 5 == 5 ? 3 : 2";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            var y = 3 == 2 ? 4 : 5 == 5 ? 3 : 2;
            Assert.AreEqual(ret, 3);
            Assert.AreEqual(ret, y);
        }

        [TestMethod]
        public void OperatorPrecedence()
        {
            var str = "1 + 2 * 3";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            var y = 1 + 2 * 3;
            Assert.AreEqual(ret, 7);
            Assert.AreEqual(ret, y);
        }

        [TestMethod]
        public void BracketGrouping()
        {
            var str = "(1 + 2) * 3";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            var y = (1 + 2) * 3;
            Assert.AreEqual(ret, 9);
            Assert.AreEqual(ret, y);
        }

        [TestMethod]
        public void ParseFSuffixReturnsSingle()
        {
            var str = "2.5F";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            Assert.IsTrue(ret.GetType() == typeof(System.Single));
            Assert.IsTrue(Convert.ToSingle(ret) == 2.5F);
        }

        [TestMethod]
        public void ParseMSuffixReturnsDecimal()
        {
            var str = "2.5M";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            Assert.IsTrue(ret.GetType() == typeof(System.Decimal));
            Assert.IsTrue(Convert.ToDecimal(ret) == 2.5M);
        }

        [TestMethod]
        public void ParseLSuffixReturnsLong()
        {
            var str = "2L";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            Assert.IsTrue(ret.GetType() == typeof(System.Int64));
            Assert.IsTrue(Convert.ToInt64(ret) == 2L);
        }

        [TestMethod]
        public void AddImplicitIntegersReturnsInteger()
        {
            var str = "1 + 1";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            Assert.IsTrue(ret.GetType() == typeof(System.Int32));
            Assert.IsTrue(Convert.ToInt32(ret) == 2);
        }

        [TestMethod]
        public void Add()
        {
            var str = "1 + 1";
            var c = new CompiledExpression<int>(str);
            var ret = c.Eval();
            Assert.IsTrue(ret == 2);
        }

        [TestMethod]
        public void Subtract()
        {
            var str = "1 - 1";
            var c = new CompiledExpression<int>(str);
            var ret = c.Eval();
            Assert.IsTrue(Convert.ToInt32(ret) == 0);
        }

        public class BoxedDecimal
        {
            public bool IsTriggered { get; set; }
            public bool CanWithdraw { get; set; }
            public decimal AmountToWithdraw { get; set; }
        }

        [TestMethod]
        public void DynamicsUnboxingTest()
        {
            var bd = new BoxedDecimal() { CanWithdraw = false, AmountToWithdraw = 12m };
            dynamic e = bd;

            var isOverDrawn = e.AmountToWithdraw > 10m;
            Assert.IsTrue(isOverDrawn);

            var t = new TypeRegistry();
            t.RegisterSymbol("e", e);
            var compiler = new CompiledExpression { TypeRegistry = t, StringToParse = "e.AmountToWithdraw > 10m" };
            compiler.Compile();
            var result = (bool)compiler.Eval();
            Assert.IsTrue(result);

        }


        [TestMethod]
        public void DynamicsTest()
        {
            //
            // Expando Objects
            //
            dynamic myObj = new ExpandoObject();
            myObj.User = "testUser";
            var t = new TypeRegistry();
            t.RegisterSymbol("myObj", myObj);
            var compiler = new CompiledExpression { TypeRegistry = t, StringToParse = "myObj.User" };
            compiler.Compile();
            var result = compiler.Eval();

            Assert.AreEqual(result, "testUser"); //test pass

            //
            // Dynamic Objects
            //
            IList testList = new ArrayList();
            testList.Add(new NameValue<string>() { Name = "User", Value = "testUserdynamic" });
            testList.Add(new NameValue<string>() { Name = "Password", Value = "myPass" });
            dynamic dynamicList = new PropertyExtensibleObject(testList);

            Assert.AreEqual(dynamicList.User, "testUserdynamic"); //test pass 
            var tr = new TypeRegistry();
            tr.RegisterSymbol("dynamicList", dynamicList);

            compiler = new CompiledExpression { TypeRegistry = tr, StringToParse = "dynamicList.User" };
            compiler.Compile();
            result = compiler.Eval();

            Assert.AreEqual(result, "testUserdynamic");

        }

        [TestMethod]
        public void IfTheElseStatementList()
        {
            var a = new ClassA() { x = 1 };
            var t = new TypeRegistry();
            t.RegisterSymbol("a", a);
            var p = new CompiledExpression { StringToParse = "if (a.x == 1) a.y = 2; else { a.y = 3; } a.z = a.y;", TypeRegistry = t };
            p.ExpressionType = CompiledExpressionType.StatementList;
            var f = p.Eval();
            Assert.AreEqual(a.y, 2);
            Assert.AreEqual(a.y, a.z);
        }

        [TestMethod]
        public void SwitchStatement()
        {
            var a = new ClassA() { x = 1 };
            var t = new TypeRegistry();

            for (a.x = 1; a.x < 7; a.x++)
            {
                switch (a.x)
                {
                    case 1:
                    case 2:
                        Debug.WriteLine("Hello");
                        break;
                    case 3:
                        Debug.WriteLine("There");
                        break;
                    case 4:
                        Debug.WriteLine("World");
                        break;
                    default:
                        Debug.WriteLine("Undefined");
                        break;
                }
            }

            t.RegisterSymbol("Debug", typeof(Debug));
            var p = new CompiledExpression { StringToParse = "switch(x) { case 1: case 2: Debug.WriteLine('Hello'); break; case 3: Debug.WriteLine('There'); break; case 4: Debug.WriteLine('World'); break; default: Debug.WriteLine('Undefined'); break; }", TypeRegistry = t };
            p.ExpressionType = CompiledExpressionType.StatementList;
            var func = p.ScopeCompile<ClassA>();
            for (a.x = 1; a.x < 7; a.x++)
            {
                func(a);
            }
        }

        //[TestMethod]
        //public void Return()
        //{
        //    var t = new TypeRegistry();

        //    var p = new CompiledExpression<bool> { StringToParse = "return true;", TypeRegistry = t };
        //    p.ExpressionType = CompiledExpressionType.StatementList;
        //    Assert.AreEqual(true, p.Compile()());

        //    p.StringToParse = "var x = 3; if (x == 3) { return true; } return false;";
        //    Assert.AreEqual(true, p.Compile()());

        //    p.StringToParse = "var x = 2; if (x == 3) { return true; } ";
        //    Assert.AreEqual(true, p.Compile()());

        //    p.StringToParse = "var x = true; x;";
        //    Assert.AreEqual(true, p.Compile()());
        //}

        //[TestMethod]
        //public void SwitchReturn()
        //{
        //    var a = new ClassA() { x = 1 };
        //    var t = new TypeRegistry();

        //    var p = new CompiledExpression { StringToParse = "var retval = 'Exit'; switch(x) { case 1: case 2: return 'Hello'; case 3: return 'There'; case 4: return 'World'; default: return 'Undefined'; } return retval;", TypeRegistry = t };
        //    p.ExpressionType = CompiledExpressionType.StatementList;
        //    var func = p.ScopeCompile<ClassA>();
        //    for (a.x = 1; a.x < 7; a.x++)
        //    {
        //        Debug.WriteLine(func(a));
        //    }
        //}

        [TestMethod]
        public void LocalImplicitVariables()
        {
            var registry = new TypeRegistry();

            object obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            var cc = new CompiledExpression() { StringToParse = "var x = new objHolder(); x.number = 3; x.number++; var varname = 23; varname++; obj.number = varname -  x.number;", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            var result = cc.Eval();
        }

        [TestMethod]
        public void WhileLoop()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            var cc = new CompiledExpression() { StringToParse = "while (obj.number < 10) { obj.number++; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(10, obj.number);
        }


        [TestMethod]
        public void ForLoop()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            for (var i = 0; i < 10; i++) { obj.number2++; }

            var cc = new CompiledExpression() { StringToParse = "for(var i = 0; i < 10; i++) { obj.number++; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(obj.number2, obj.number);
        }

        [TestMethod]
        public void ForLoopWithMultipleIterators()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            for (int i = 0, j = 0; i < 10; i++, j++) { obj.number2 = j; }

            var cc = new CompiledExpression() { StringToParse = "for(int i = 0, j = 0; i < 10; i++, j++) { obj.number = j; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(9, obj.number);
            Assert.AreEqual(obj.number2, obj.number);
        }

        [TestMethod]
        public void ForEachLoop()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { iterator = new List<string>() { "Hello", "there", "world" } };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("Debug", typeof(Debug));
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            //var iterator = new List<string>() { "Hello", "there", "world" };
            //var enumerator = iterator.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    var word = enumerator.Current;
            //    Debug.WriteLine(word);
            //}

            var cc = new CompiledExpression() { StringToParse = "foreach(var word in obj.iterator) { Debug.WriteLine(word); }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
        }

        [TestMethod]
        public void ForEachLoopNoBlock()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { iterator = new List<string>() { "Hello", "there", "world" } };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("Debug", typeof(Debug));
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            //var iterator = new List<string>() { "Hello", "there", "world" };
            //var enumerator = iterator.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    var word = enumerator.Current;
            //    Debug.WriteLine(word);
            //}

            var cc = new CompiledExpression() { StringToParse = "foreach(var word in obj.iterator) Debug.WriteLine(word);", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
        }


        [TestMethod]
        public void ForEachLoopArray()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { stringIterator = new[] { "Hello", "there", "world" } };

            //foreach (var word in obj.stringIterator) { Debug.WriteLine(word); }

            var enumerator = obj.stringIterator.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string word = (string)enumerator.Current;
                Debug.WriteLine(word);
            }

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("Debug", typeof(Debug));
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            var cc = new CompiledExpression() { StringToParse = "foreach(var word in obj.stringIterator) { Debug.WriteLine(word); }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
        }


        [TestMethod]
        public void ForLoopWithContinue()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };
            var obj2 = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            for (var i = 0; i < 10; i++) { obj2.number++; if (i > 5) continue; obj2.number2++; }

            var cc = new CompiledExpression() { StringToParse = "for(var i = 0; i < 10; i++) { obj.number++; if(i > 5) continue; obj.number2++; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(obj2.number, obj.number);
            Assert.AreEqual(obj2.number2, obj.number2);
        }

        [TestMethod]
        public void ForLoopWithBreak()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };
            var obj2 = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            for (var i = 0; i < 10; i++) { obj2.number++; if (i > 5) break; obj2.number2++; }

            var cc = new CompiledExpression() { StringToParse = "for(var i = 0; i < 10; i++) { obj.number++; if(i > 5) break; obj.number2++; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(obj2.number, obj.number);
            Assert.AreEqual(obj2.number2, obj.number2);
        }

        [TestMethod]
        public void WhileLoopWithBreak()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            var cc = new CompiledExpression() { StringToParse = "while (obj.number < 10) { obj.number++; if(obj.number == 5) break; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(5, obj.number);
        }

        [TestMethod]
        public void NestedWhileLoopWithBreak()
        {
            var registry = new TypeRegistry();

            var obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("Debug", typeof(Debug));
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();
            var cc = new CompiledExpression() { StringToParse = "while (obj.number < 10) { Debug.WriteLine((object)obj.number); obj.number++; while (obj.number2 < 10) { Debug.WriteLine((object)obj.number2); obj.number2++; if(obj.number2 == 5) break;  }  if(obj.number == 5) break; }", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            cc.Eval();
            Assert.AreEqual(5, obj.number);
            Assert.AreEqual(10, obj.number2);
        }

        [TestMethod]
        public void ScopeCompileTypedResultTypedParam()
        {
            var scope = new ClassA() { x = 1 };
            var target = new CompiledExpression<int>("x");
            target.ScopeCompile<ClassA>();
        }

        [TestMethod]
        public void ScopeCompileTypedResultObjectParam()
        {
            var scope = new ClassA() { x = 1 };
            var target = new CompiledExpression<int>("1");
            target.ScopeCompile();
        }

        [TestMethod]
        public void AssignmentOperators()
        {
            var classA = new ClassA();

            var exp = new CompiledExpression();
            Func<ClassA, object> func;

            exp.StringToParse = "x = 1";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(1, classA.x);

            exp.StringToParse = "x += 9";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(10, classA.x);

            exp.StringToParse = "x -= 4";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(6, classA.x);

            exp.StringToParse = "x *= 5";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(30, classA.x);

            exp.StringToParse = "x /= 2";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(15, classA.x);

            exp.StringToParse = "x %= 13";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(2, classA.x);

            exp.StringToParse = "x <<= 4";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(32, classA.x);

            exp.StringToParse = "x >>= 1";
            func = exp.ScopeCompile<ClassA>();
            func(classA);
            Assert.AreEqual(16, classA.x);
        }


        [TestMethod]
        public void MethodOverLoading()
        {
            var controlScope = new MethodOverloading();
            var testScope = new MethodOverloading();

            var exp = new CompiledExpression();
            Func<object, object> func;

            controlScope.sum(1, 2, 3, 4, 5, 6, 7, 8);

            exp.StringToParse = "sum(1, 2, 3, 4, 5, 6, 7, 8)";
            func = exp.ScopeCompile();
            func(testScope);
            // expect sum(float i, params float[] nums) 
            Assert.AreEqual(controlScope.MethodCalled, testScope.MethodCalled);

            controlScope.sum(1, 2);

            exp.StringToParse = "sum(1, 2)";
            func = exp.ScopeCompile();
            func(testScope);
            // expect sum(int,int) is called
            Assert.AreEqual(controlScope.MethodCalled, testScope.MethodCalled);

            controlScope.sum(1.0d, 2.0d);

            exp.StringToParse = "sum(1.0d, 2.0d)";
            func = exp.ScopeCompile();
            func(testScope);
            // expect sum(double, double) is called
            Assert.AreEqual(controlScope.MethodCalled, testScope.MethodCalled);

            controlScope.sum(1, 2.0d);

            exp.StringToParse = "sum(1,2.0d)";
            func = exp.ScopeCompile();
            func(testScope);
            // expect sum(double, double) is called (no matching int, double)
            Assert.AreEqual(controlScope.MethodCalled, testScope.MethodCalled);
        }

        //[TestMethod]
        //public void Lambda()
        //{
        //    var tr = new TypeRegistry();
        //    tr.RegisterType("Enumerable", typeof(Enumerable));
        //    var data = new MyClass();
        //    data.Y = new List<int>() { 1, 2, 3, 4, 5, 4, 4, 3, 4, 2 };
        //    var c9 = new CompiledExpression() { StringToParse = "Enumerable.Where<int>(Y, (y) => y == 4)", TypeRegistry = tr };
        //    var f9 = c9.ScopeCompile<MyClass>();

        //    Console.WriteLine(data.X);
        //    f9(data);
        //    Console.WriteLine(data.X);
        //}

        [TestMethod]
        public void CompileToGenericFunc()
        {
            var data = new MyClass();
            data.Y = new List<int>() { 1, 2, 3, 4, 5, 4, 4, 3, 4, 2 };
            var c9 = new CompiledExpression() { StringToParse = "y == 4" };
            var f9 = c9.Compile<Func<int, bool>>("y");
            Assert.AreEqual(4, data.Y.Where(f9).Count());
        }

        [TestMethod]
        public void DynamicValue()
        {
            var registry = new TypeRegistry();
            var obj = new objHolder() { Value = "aa" };
            registry.RegisterSymbol("obj", obj);
            registry.RegisterDefaultTypes();

            var cc = new CompiledExpression() { StringToParse = "obj.Value == 'aa'", TypeRegistry = registry };
            var ret = cc.Eval();
            Assert.AreEqual(true, ret);

            obj.Value = 10;
            var test = obj.Value == 10;
            cc = new CompiledExpression() { StringToParse = "obj.Value == 10", TypeRegistry = registry };
            ret = cc.Eval();
            Assert.AreEqual(true, ret);

            obj.Value = 10.0;
            cc = new CompiledExpression() { StringToParse = "obj.Value == 10", TypeRegistry = registry };
            ret = cc.Eval();
            Assert.AreEqual(true, ret);

            obj.Value = 10.0;
            cc = new CompiledExpression() { StringToParse = "obj.Value = 5", TypeRegistry = registry };
            ret = cc.Eval();
            Assert.AreEqual(5, obj.Value);

            obj.Value = 10;
            cc = new CompiledExpression() { StringToParse = "obj.Value == 10.0", TypeRegistry = registry };
            ret = cc.Eval();
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void NullableType()
        {
            var expression = new CompiledExpression()
            {
                TypeRegistry = new TypeRegistry()
            };

            int? argument1 = 5;
            var argument2 = new Fact()
            {
                Count = 5
            };

            expression.TypeRegistry.RegisterSymbol("Argument1", argument1, typeof(int?));
            expression.TypeRegistry.RegisterSymbol("Argument2", argument2);

            // Works
            expression.StringToParse = "Argument2.Count != null";
            expression.Eval();

            // Fails with NullReferenceException
            expression.StringToParse = "Argument1 != null";
            expression.Eval();
        }

        [TestMethod]
        public void OverloadedBinaryOperators()
        {
            var registry = new TypeRegistry();
            var target = new CompiledExpression() { TypeRegistry = registry };

            var x = new TypeWithOverloadedBinaryOperators(3);
            registry.RegisterSymbol("x", x);

            string y = "5";
            Assert.IsFalse(x == y);
            target.StringToParse = "x == y";
            Assert.IsFalse(target.Compile<Func<string, bool>>("y")(y));

            y = "3";
            Assert.IsTrue(x == y);
            target.StringToParse = "x == y";
            Assert.IsTrue(target.Compile<Func<string, bool>>("y")(y));

            target.StringToParse = "x == \"4\"";
            Assert.IsFalse(target.Compile<Func<bool>>()());
            target.StringToParse = "x == \"3\"";
            Assert.IsTrue(target.Compile<Func<bool>>()());
        }

        struct TypeWithOverloadedBinaryOperators
        {
            private int _value;

            public TypeWithOverloadedBinaryOperators(int value)
            {
                _value = value;
            }

            public static bool operator ==(TypeWithOverloadedBinaryOperators instance, string value)
            {
                return instance._value.ToString().Equals(value);
            }

            public static bool operator !=(TypeWithOverloadedBinaryOperators instance, string value)
            {
                return !instance._value.ToString().Equals(value);
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (obj is TypeWithOverloadedBinaryOperators)
                {
                    return this._value.Equals(((TypeWithOverloadedBinaryOperators)obj)._value);
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ExpressionParseException))]
        public void ExpressionException()
        {
            var c = new CompiledExpression() ;
            c.StringToParse = "(1 + 2))";
            var result = c.Eval();
        }


        [TestMethod]
        [ExpectedException(typeof(ExpressionParseException))]
        public void ExpressionException2()
        {
            var c = new CompiledExpression();
            c.StringToParse = "25L +";
            var result = c.Eval();
        }

        [TestMethod]
        [ExpectedException(typeof(ExpressionParseException))]
        public void ExpressionException3()
        {
            var c = new CompiledExpression();
            c.StringToParse = "25.L";
            var result = c.Eval();
        }
    }

    public class Fact
    {
        public int? Count { get; set; }
    }

    public class MyClass
    {
        public int X { get; set; }
        public List<int> Y { get; set; }
        public Func<bool> Value { get; set; }
        public void Foo()
        {
            X++;
        }

        public void Foo(int value)
        {
            X += value;
        }

        public int Bar(int value)
        {
            return value * 2;
        }
    }

    public class P
    {
        public bool f1(ScopeDataType data)
        {
            return false;
        }
        public bool f2(ScopeDataType data)
        {
            return false;
        }
        public bool f3(ScopeDataType data)
        {
            return false;
        }
        public bool f4(ScopeDataType data)
        {
            return true;
        }
        //public Func<ScopeDataType, bool> f1 { get; set; }
        //public Func<ScopeDataType, bool> f2 { get; set; }
        //public Func<ScopeDataType, bool> f3 { get; set; }
        //public Func<ScopeDataType, bool> f4 { get; set; }
    }

    public class ScopeDataType
    {
        public ScopeDataType data
        {
            get { return this; }
        }
    }

    public class objHolder
    {
        public bool result { get; set; }
        public NumEnum value { get; set; }
        public int number { get; set; }
        public int number2 { get; set; }
        public IEnumerable<string> iterator;
        public IEnumerable objectIterator;
        public string[] stringIterator;
        public dynamic Value;
    }

    public enum NumEnum
    {
        One = 1,
        Two = 2,
        Three = 3
    }


    public class ClassA
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class NameValue<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
    }

    public class MethodOverloading
    {
        public int MethodCalled { get; set; }

        public double sum(double i, double t)
        {
            MethodCalled = 1;
            var result = 0d;
            return result;
        }

        public double sum(double i, int t)
        {
            MethodCalled = 2;
            var result = 0d;
            return result;
        }

        public int sum(int i, int t)
        {
            MethodCalled = 3;
            var result = 0;
            return result;
        }

        public int sum(int i1, int i2, int i3, int i4, int i5)
        {
            MethodCalled = 4;
            var result = 0;
            return result;
        }


        public double sum(double i, params double[] nums)
        {
            MethodCalled = 5;
            var result = 0d;
            foreach (var num in nums)
            {
                result += num;
            }
            return result;
        }

        public float sum(float i, params float[] nums)
        {
            MethodCalled = 6;
            var result = 0f;
            foreach (var num in nums)
            {
                result += num;
            }
            return result;
        }


        public int yes()
        {
            return 1234;
        }

        public bool no
        {
            get { return false; }
        }

        public int fix(int x)
        {
            return x + 1;
        }

        public int func(Predicate<int> t)
        {
            return t(5) ? 1 : 2;
        }
    }

}
