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
            var c = new CompiledExpression();
            c.StringToParse = "'1000.00'.Replace('.', '')";
            var e = c.Eval(); // returns 100000

            c.StringToParse = "'1,000.00'.Replace(',', '.')";
            e = c.Eval(); // returns 1.000.00

            c.RegisterDefaultTypes();
            c.RegisterType("CultureInfo", typeof(CultureInfo));

            c.StringToParse = "DateTime.ParseExact('02/11/2014 09:14', 'M/d/yyyy hh:mm', CultureInfo.InvariantCulture).ToString('dd/MM/yyyy HH:MM:SS')";
            e = c.Eval(); // returns  11/02/2014 09:02:SS

            dynamic scope = new ExpandoObject();
            scope.x = 2;
            scope.y = 3;

            var q = new c();
            var qq = new c2();

            var x = q.sum(1f, 2f);

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
