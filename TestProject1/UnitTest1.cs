using System;
using System.Collections;
using System.Dynamic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressionEvaluator;
using UnitTestProject1;

namespace ExpressionEvaluator.Tests
{
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
        [ExpectedException(typeof(Exception))]
        public void ParseInvalidNumericThrowsException()
        {            
            var str = "2.55X";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
        }

        [TestMethod]
        public void UnderscoreVariables()
        {
            var str = "1 | VARIABLE_NAME | _VARNAME";
            var c = new CompiledExpression(str);
            c.RegisterType("VARIABLE_NAME", 16);
            c.RegisterType("_VARNAME", 32);
            var ret = c.Eval();
        }


        public class Xer
        {
            public int X { get; set; }
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
            var  y = 1 + 2 * 3;
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

        [TestMethod]
        public void NameValueTest()
        {
            //
            // Expando Objects
            //
            dynamic myObj = new ExpandoObject();
            myObj.User = "testUser";

            CompiledExpression compiler = new CompiledExpression();
            compiler.RegisterType("myObj", myObj);
            compiler.StringToParse = "myObj.User";
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

            compiler = new CompiledExpression();
            compiler.RegisterType("dynamicList", dynamicList);
            compiler.StringToParse = "dynamicList.User";
            compiler.Compile();   //!!!!!!FAILS here with this message : Member not found: PropertyExtensibleObject.User
            result = compiler.Eval();

            Assert.AreEqual(result, "testUserdynamic");

        }

    }

    public class NameValue<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
    }
}
