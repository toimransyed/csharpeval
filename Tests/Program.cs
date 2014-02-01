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
    }

    class Program
    {

        static void Main(string[] args)
        {
            var c = new CompiledExpression("b.no || true");

            dynamic scope = new ExpandoObject();
            scope.x = 2;
            scope.y = 3;

            var q = new c();

            scope.b = q;

            var f = c.ScopeCompile();
            var r = f(scope);

            Console.WriteLine(r);

            scope.x = 3;
            r = f(scope);

            Console.WriteLine(r);


            Console.ReadLine();
        }
    }
}
