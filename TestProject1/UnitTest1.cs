using System;
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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

        //[TestMethod]
        //[ExpectedException(typeof(Exception))]
        //public void ParseInvalidNumericThrowsException()
        //{
        //    var str = "2.55 + 32";
        //    var c = new CompiledExpression(str);
        //    var ret = c.Eval();
        //}

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
            t.RegisterSymbol("TestClass", typeof(TestClass));
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
}
