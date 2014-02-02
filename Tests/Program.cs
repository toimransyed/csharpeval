using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;

namespace Tests
{
    public class c
    {
        public double sum(double i, params double[] nums)
        {
            var result = 0d;
            foreach (var num in nums)
            {
                result += num;
            }
            return result;
        }

        //public int sum(int i, params int[] nums)
        //{
        //    var result = 0;
        //    foreach (var num in nums)
        //    {
        //        result += num;
        //    }
        //    return result;
        //}

        
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
            var c = new CompiledExpression("sum(1,2,3,4,5)");

            dynamic scope = new ExpandoObject();
            scope.x = 2;
            scope.y = 3;

            var q = new c();
            var qq = new c2();

            var x = q.sum(1, 2, 3, 4, 5);

            scope.b = q;

            var f = c.ScopeCompile<c>();

            var r = f(q);
            //var rr = f(qq);

            Console.WriteLine(r);
            //Console.WriteLine(rr);

            scope.x = 3;
            r = f(q);

            Console.WriteLine(r);


            Console.ReadLine();
        }
    }
}
