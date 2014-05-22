using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;

namespace REPLSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var repl = new REPL();
            repl.Loop();
        }
    }

    class Context
    {
        
    }

    class REPL
    {
        private bool isRunning;
        private CompiledExpression cex;
        private List<string> history;
        private string expression;
        private object result;
        public REPL()
        {
            history = new List<string>();
            cex = new CompiledExpression();
            cex.TypeRegistry = new TypeRegistry();
            cex.TypeRegistry.RegisterDefaultTypes();
            cex.ExpressionType = CompiledExpressionType.Expression;
            isRunning = true;
        }

        public virtual string Read()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }

        public virtual object Eval(string text)
        {
            cex.StringToParse = text;
            return cex.Eval();
        }

        public virtual void Print(object value)
        {
            Console.WriteLine(value);
        }

        public void Loop()
        {
            while (isRunning)
            {
                try
                {
                    Print(Eval(Read()));
                }
                catch (Exception ex)
                {
                    Print(ex.Message);
                }
            }
        }
    }
}
