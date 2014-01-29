using System.Linq.Expressions;
using ExpressionEvaluator.Tokens;

namespace ExpressionEvaluator.Operators
{
    internal class OpFuncServiceProviders
    {
        public static Expression MethodOperatorFunc(
            OpFuncArgs args
            )
        {
            string nextToken = ((MemberToken)args.T).Name;
            Expression le = args.ExprStack.Pop();

            Expression result = ((MethodOperator)args.Op).Func(le, nextToken, args.Args);

            return result;
        }

        public static Expression TypeOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression le = args.ExprStack.Pop();
            return ((TypeOperator)args.Op).Func(le, args.T.Type);
        }

        public static Expression UnaryOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression le = args.ExprStack.Pop();
            return ((UnaryOperator)args.Op).Func(le);
        }

        public static Expression BinaryOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression re = args.ExprStack.Pop();
            Expression le = args.ExprStack.Pop();
            // perform implicit conversion on known types
            TypeConversion.Convert(ref le, ref re);
            return ((BinaryOperator)args.Op).Func(le, re);
        }

        public static Expression TernaryOperatorFunc(OpFuncArgs args)
        {
            Expression falsy = args.ExprStack.Pop();
            Expression truthy = args.ExprStack.Pop();
            Expression condition = args.ExprStack.Pop();
            // perform implicit conversion on known types ???
            TypeConversion.Convert(ref falsy, ref truthy);
            return ((TernaryOperator)args.Op).Func(condition, truthy,falsy);
        }

        public static Expression TernarySeparatorOperatorFunc(OpFuncArgs args)
        {
            return args.ExprStack.Pop();
        }

    }
}