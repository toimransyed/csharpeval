using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ExpressionEvaluator;

namespace Tests
{

    public class c2
    {
        public int yes()
        {
            return 1234;
        }

        public bool no
        {
            get { return true; }
        }

        public int fix(int x)
        {
            return x + 1;
        }

        public int sum(params int[] nums)
        {
            var result = 0;
            foreach (var num in nums)
            {
                result -= num;
            }
            return result;
        }

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

    public class objHolder
    {
        public bool result { get; set; }
        public NumEnum value { get; set; }
        public int number { get; set; }
    }

    public enum NumEnum
    {
        One = 1,
        Two = 2,
        Three = 3
    }

    public class ValueHolder
    {
        public string Value { get; set; }
    }

    public class Utility
    {
        public float ATADJ(float value, float adj)
        {
            return value;
        }

        public float MAX(float value1, float value2)
        {
            return new List<float> { value1, value2 }.Max();
        }

        public float MAX(params float[] value)
        {
            return value.Max();
        }

        public float SUBTR(float left, float right)
        {
            return left - right;
        }

        public float MULT(float left, float right)
        {
            return left * right;
        }

        public float PR(float value)
        {
            return value;
        }

        public float AMT(float value)
        {
            return value;
        }
    }

    public class objHolder2
    {
        public dynamic Value;
    }
    class Program
    {
        static void Main(string[] args)
        {
            var obj2 = new objHolder2();
            obj2.Value = 5;
            Type t = Type.GetType("Mono.Runtime");
            if (t != null)
                Console.WriteLine("Mono.Runtime detected");
            else
                Console.WriteLine("Not Mono");

            var exp = "@ATADJ( @MAX( @SUBTR(@PR( 987043 ) , @AMT( 913000 ) ) , @MULT( @PR( 987043 ) , 0.20f ) ) , 60f ) ";
            var util = new Utility();
            var reg = new TypeRegistry();
            reg.RegisterSymbol("util", util);
            exp = exp.Replace("@", "util.");
            var ce = new CompiledExpression() { StringToParse = exp, TypeRegistry = reg };
            var res = ce.Eval();

            var registry1 = new TypeRegistry();
            var registry = new TypeRegistry();

            object obj = new objHolder() { result = false, value = NumEnum.Two };

            registry.RegisterSymbol("obj", obj);
            registry.RegisterType("objHolder", typeof(objHolder));
            registry.RegisterDefaultTypes();

            var cc = new CompiledExpression() { StringToParse = "var x = new objHolder(); x.number = 3; x.number++; var varname = 23; varname++; obj.number = varname -  x.number;", TypeRegistry = registry };
            cc.ExpressionType = CompiledExpressionType.StatementList;
            var result = cc.Eval();


            var field = new ValueHolder() { Value = "405" };
            var query = new ValueHolder() { Value = "405" };
            var ra11 = Convert.ToInt32(field.Value) >= Convert.ToInt32(query.Value);
            registry1.RegisterSymbol("field", field);
            registry1.RegisterSymbol("query", query);
            registry1.RegisterDefaultTypes();
            var c11 = new CompiledExpression() { StringToParse = "Convert.ToInt32(field.Value) >= Convert.ToInt32(query.Value)", TypeRegistry = registry1 };
            var x11 = c11.Compile();
            var r11 = x11();

            var cy = new CompiledExpression() { StringToParse = "int.Parse(\"1000\")", TypeRegistry = registry };
            var resulty = cy.Eval();

            var x = new List<String>() { "Hello", "There", "World" };
            dynamic scope = new ExpandoObject();
            scope.x = x;
            var data = new MyClass { Value = () => false, Y = new List<int>() { 1, 2, 3, 4, 4, 5, 6, 4, 2, 3 } };
            var item = new MyClass { Value = () => true };
            scope.data = data;
            scope.item = item;
            scope.i = 1;
            var a = scope.data.Value() && scope.item.Value();
            //var b = !scope.data.Value() || scope.item.Value();

            registry.RegisterSymbol("data", data);


            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            var pi = Convert.ToString(3.141592654);
            var xs = 2d;
            var pipi = 3.141592654.ToString();
            var c0 = new CompiledExpression() { StringToParse = "3.141592654.ToString()" };
            var pi2 = c0.Eval();

            var p = scope.x[0];

            // (data.Value && !item.Value) ? 'yes' : 'no'
            var c = new CompiledExpression() { StringToParse = "data.Foo(30 + data.Bar(10))", TypeRegistry = registry };
            Console.WriteLine(data.X);
            c.Eval();
            //c.Call();
            Console.WriteLine(data.X);

            var c8 = new CompiledExpression() { StringToParse = "data.X  + '%'", TypeRegistry = registry };
            Console.WriteLine(data.X);
            var cr = c8.Eval();
            Console.WriteLine(data.X);


            var c1 = new CompiledExpression() { StringToParse = "Foo()" };
            var f1 = c1.ScopeCompileCall<MyClass>();
            Console.WriteLine(data.X);
            f1(data);
            Console.WriteLine(data.X);


            var qq = (25.82).ToString("0.00", new CultureInfo("fr-FR")) + "px";
            var test = "(25.82).ToString('0.00') + 'px'";
            var cx = new CompiledExpression() { StringToParse = "int.Parse('25.82', new CultureInfo(\"fr-FR\"))" };


            var c2 = new CompiledExpression() { StringToParse = "data.Foo();" };
            var y = 12 + "px";
            var f2 = c2.ScopeCompileCall();
            Console.WriteLine(scope.data.X);
            f2(scope);
            Console.WriteLine(scope.data.X);


            Console.ReadLine();
        }
    }
}
