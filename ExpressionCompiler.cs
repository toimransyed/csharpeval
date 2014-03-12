using System.Linq.Expressions;

namespace ExpressionEvaluator
{
    public abstract class ExpressionCompiler
    {
        protected Expression Expression = null;
        protected AntlrParser Parser = null;
        protected TypeRegistry TypeRegistry = new TypeRegistry();
        protected string Pstr = null;

        public string StringToParse
        {
            get { return Parser.ExpressionString; }
            set {
                Parser.ExpressionString = value;
                Expression = null;
                ClearCompiledMethod();
            }
        }

        public void RegisterDefaultTypes()
        {
            TypeRegistry.RegisterDefaultTypes();
        }

        public void RegisterType(string key, object type)
        {
            TypeRegistry.Add(key, type);
        }

        protected Expression BuildTree(Expression scopeParam = null, bool isCall = false)
        {
            return Expression = Parser.Parse(scopeParam, isCall);
        }

        protected abstract void ClearCompiledMethod();

        protected void Parse()
        {
            BuildTree(null, false);
        }

        public void RegisterNamespace(string p)
        {
        }

        public void RegisterAssembly(System.Reflection.Assembly assembly)
        {
        }

    }
}