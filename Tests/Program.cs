using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "3 == 2 ? 4 : 5 == 5 ? 3 : 2";
            var c = new CompiledExpression(str);
            var ret = c.Eval();
            var x = 3 == 2 ? 4 : 5 + 2;
            var y = 3 == 2 ? 4 : 5 == 5 ? 3 : 2;

            Console.WriteLine(ret);
            Console.ReadLine();
        }
    }
}
