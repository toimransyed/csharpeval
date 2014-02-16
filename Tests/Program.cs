using System;
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



    class Program
    {

        static void Main(string[] args)
        {
            var x = new List<String>() { "Hello", "There", "World" };
            dynamic scope = new ExpandoObject();
            scope.x = x;
            var p = scope.x[0];


            var c = new CompiledExpression() { StringToParse = "x[0] + ', ' + x[2]" };
            var f = c.ScopeCompile();

            Console.WriteLine(f(scope));



            Console.ReadLine();
        }
    }
}
