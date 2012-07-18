using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;


namespace ExpressionEvaluator
{

    internal class MethodOperator : Operator<Func<Expression, string, List<Expression>, Expression>>
    {
        public MethodOperator(string value, int precedence, bool leftassoc, Func<Expression, string, List<Expression>, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
        }
    }

    internal class BinaryOperator : Operator<Func<Expression, Expression, Expression>>
    {
        public BinaryOperator(string value, int precedence, bool leftassoc, Func<Expression, Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
            arguments = 2;
        }
    }

    internal class UnaryOperator : Operator<Func<Expression, UnaryExpression>>
    {
        public UnaryOperator(string value, int precedence, bool leftassoc, Func<Expression, UnaryExpression> func)
            : base(value, precedence, leftassoc, func)
        {
            arguments = 1;
        }
    }

    internal class TypeOperator : Operator<Func<Expression, Type, UnaryExpression>>
    {
        public TypeOperator(string value, int precedence, bool leftassoc, Func<Expression, Type, UnaryExpression> func)
            : base(value, precedence, leftassoc, func)
        {
            arguments = 1;
        }
    }

    internal interface IOperator
    {
        string value { get; set; }
        int precedence { get; set; }
        int arguments { get; set; }
        bool leftassoc { get; set; }
    }

    internal abstract class Operator<T> : IOperator
    {
        public T func;

        public string value { get; set; }
        public int precedence { get; set; }
        public int arguments { get; set; }
        public bool leftassoc { get; set; }

        public Operator(string value, int precedence, bool leftassoc, T func)
        {
            this.value = value;
            this.precedence = precedence;
            this.leftassoc = leftassoc;
            this.func = func;
        }

    }


    internal class OperatorCollection : Dictionary<string, IOperator>
    {
        List<char> firstlookup = new List<char>();

        public new void Add(string key, IOperator op)
        {
            firstlookup.Add(key[0]);
            base.Add(key, op);
        }

        public bool ContainsFirstKey(char key)
        {
            return firstlookup.Contains(key);
        }
    }

    internal delegate Expression OpFuncDelegate(
        OpFuncArgs args
    );

    internal class OpFuncServiceLocator
    {
        public OpFuncServiceLocator()
        {
            typeActions.Add(typeof(MethodOperator), OpFuncServiceProviders.MethodOperatorFunc);
            typeActions.Add(typeof(TypeOperator), OpFuncServiceProviders.TypeOperatorFunc);
            typeActions.Add(typeof(UnaryOperator), OpFuncServiceProviders.UnaryOperatorFunc);
            typeActions.Add(typeof(BinaryOperator), OpFuncServiceProviders.BinaryOperatorFunc);
        }

        public OpFuncDelegate Resolve(Type type)
        {
            return typeActions[type];
        }

        public Dictionary<Type, OpFuncDelegate>
            typeActions = new Dictionary<Type, OpFuncDelegate>();
    }

    internal class OpFuncArgs
    {
        public Queue<Token> tempQueue;
        public Stack<Expression> exprStack;
        public Stack<String> literalStack;
        public Token t;
        public IOperator op;
        public List<Expression> args;
    }

    internal class OpFuncServiceProviders
    {
        public static Expression MethodOperatorFunc(
            OpFuncArgs args
          )
        {
            var nextToken = args.literalStack.Pop();

            Expression le = args.exprStack.Pop();

            Expression result = ((MethodOperator)args.op).func(le, nextToken, args.args);

            return result;
        }

        public static Expression TypeOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression le = args.exprStack.Pop();
            return ((TypeOperator)args.op).func(le, args.t.type);
        }

        public static Expression UnaryOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression le = args.exprStack.Pop();
            return ((UnaryOperator)args.op).func(le);
        }

        public static Expression BinaryOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression re = args.exprStack.Pop();
            Expression le = args.exprStack.Pop();
            // check if these expressions can be implicitly converted
            Parser.ImplicitConversion(ref le, ref re);
            return ((BinaryOperator)args.op).func(le, re);
        }


    }

}
