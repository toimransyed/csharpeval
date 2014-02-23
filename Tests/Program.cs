using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;

namespace Tests
{
    public class c
    {
        public double sum(double i, double t)
        {
            var result = 0d;
            return result;
        }

        public double sum(double i, int t)
        {
            var result = 0d;
            return result;
        }

        public int sum(int i, int t)
        {
            var result = 0;
            return result;
        }

        public int sum(int i1, int i2, int i3, int i4, int i5)
        {
            var result = 0;
            return result;
        }


        public double sum(double i, params double[] nums)
        {
            var result = 0d;
            foreach (var num in nums)
            {
                result += num;
            }
            return result;
        }

        public float sum(float i, params float[] nums)
        {
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
        public Func<bool> Value { get; set; }
    }

    public class Generic
    {
        public int GenericMethod<T, U>(U arg1, T arg2)
            where T : IList
            where U : IEnumerable
        {
            return 0;
        }
    }

    public class Scope
    {
        public Generic g { get; set; }
        public IList x { get; set; }
        public int i { get; set; }
    }

    class Program
    {

        static void Main(string[] args)
        {
            var x = new List<String>() { "Hello", "There", "World" };
            var scope = new Scope();
            var g = new Generic();
            scope.x = x;
            scope.g = g;

            scope.i = 0;
            var data = new MyClass { Value = () => false };
            var item = new MyClass { Value = () => true };
            //scope.data = data;
            //scope.item = item;
            var tt = Enumerable.First<string>((IList<string>)scope.x);

            //var a = scope.data.Value() && scope.item.Value();
            //var b = !scope.data.Value() || scope.item.Value();
            var r = new Random();

            //scope.r = r;



            var p = scope.x[0];

            // (data.Value && !item.Value) ? 'yes' : 'no'

            var c = new CompiledExpression() { StringToParse = "Enumerable.First(x)" };
            c.RegisterType("Enumerable", typeof(Enumerable));
            var f = c.ScopeCompile<Scope>();
            g.GenericMethod<List<string>, IList>(x, x);

            for (int j = 0; j < 3; j++)
            {
                scope.i = j;
                Console.WriteLine(f(scope));
            }



            Console.ReadLine();
        }
    }
}
