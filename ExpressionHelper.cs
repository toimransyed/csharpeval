using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ExpressionEvaluator.Operators;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace ExpressionEvaluator
{
    public class ExpressionHelper
    {
        private static readonly Type StringType = typeof(string);
        private static readonly MethodInfo ToStringMethodInfo = typeof(Convert).GetMethod("ToString", new Type[] { typeof(CultureInfo) });


        private static Expression ConvertToString(Expression instance)
        {
            return Expression.Call(typeof(Convert), "ToString", null, instance, Expression.Constant(CultureInfo.InvariantCulture));
        }

        public static Expression Add(Expression le, Expression re)
        {
            if (le.Type == StringType || re.Type == StringType)
            {

                if (le.Type != typeof(string)) le = ConvertToString(le);
                if (re.Type != typeof(string)) re = ConvertToString(re);
                return Expression.Add(le, re, StringType.GetMethod("Concat", new Type[] { le.Type, re.Type }));
            }
            else
            {
                return Expression.Add(le, re);
            }
        }

        public static Expression GetPropertyIndex(Expression le, IEnumerable<Expression> args)
        {
            Expression instance = null;
            Type type = null;

            var isDynamic = false;
            var isRuntimeType = false;

            if (le.Type.Name == "RuntimeType")
            {
                isRuntimeType = true;
                type = ((Type)((ConstantExpression)le).Value);
            }
            else
            {
                type = le.Type;
                instance = le;
                isDynamic = type.IsDynamic();
            }

            if (isDynamic)
            {
                var expArgs = new List<Expression>() { le };
                expArgs.AddRange(args);

                var indexedBinder = Binder.GetIndex(
                    CSharpBinderFlags.None,
                    type,
                    expArgs.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
                    );

                return Expression.Dynamic(indexedBinder, typeof(object), expArgs);

            }
            else
            {
                return Expression.ArrayAccess(le, args);
            }

        }

        public static Expression Assign(Expression le, Expression re)
        {
            // remove leading dot
            //membername = membername.Substring(1);

            Expression instance = null;
            Type type = null;

            var isDynamic = false;
            var isRuntimeType = false;

            type = le.Type;
            instance = le;
            isDynamic = type.IsDynamic();

            if (isDynamic)
            {
                var dle = (DynamicExpression)le;
                var membername = ((GetMemberBinder)dle.Binder).Name;
                instance = dle.Arguments[0];

                var binder = Binder.SetMember(
                    CSharpBinderFlags.None,
                    membername,
                    type,
                    new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }
                    );

                return Expression.Dynamic(binder, typeof(object), instance, re);

            }
            return Expression.Assign(le, re);

            throw new Exception();
        }



        public static Expression GetProperty(Expression le, string membername)
        {
            // remove leading dot
            //membername = membername.Substring(1);

            Expression instance = null;
            Type type = null;

            var isDynamic = false;
            var isRuntimeType = false;

            if (le.Type.Name == "RuntimeType")
            {
                isRuntimeType = true;
                type = ((Type)((ConstantExpression)le).Value);
            }
            else
            {
                type = le.Type;
                instance = le;
                isDynamic = type.IsDynamic();
            }

            if (isDynamic)
            {
                var binder = Binder.GetMember(
                    CSharpBinderFlags.None,
                    membername,
                    type,
                    new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }
                    );

                var result = Expression.Dynamic(binder, typeof(object), instance);

                return result;
            }
            else
            {
                Expression exp = null;

                var propertyInfo = type.GetProperty(membername);
                if (propertyInfo != null)
                {
                    exp = Expression.Property(instance, propertyInfo);
                }
                else
                {
                    var fieldInfo = type.GetField(membername);
                    if (fieldInfo != null)
                    {
                        exp = Expression.Field(instance, fieldInfo);
                    }
                }

                return exp;
            }

            throw new Exception();
        }

        public static Expression GetMethod(Expression le, string membername, List<Expression> args, bool isCall = false)
        {
            Expression instance = null;
            Type type = null;

            var isDynamic = false;
            var isRuntimeType = false;

            if (typeof(Type).IsAssignableFrom(le.Type))
            {
                isRuntimeType = true;
                type = ((Type)((ConstantExpression)le).Value);
            }
            else
            {
                type = le.Type;
                instance = le;
                isDynamic = type.IsDynamic();
            }

            if (isDynamic)
            {
                var expArgs = new List<Expression> { instance };

                expArgs.AddRange(args);

                if (isCall)
                {
                    var binderMC = Binder.InvokeMember(
                        CSharpBinderFlags.ResultDiscarded,
                        membername,
                        null,
                        type,
                        expArgs.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
                        );

                    return Expression.Dynamic(binderMC, typeof(void), expArgs);
                }

                var binderM = Binder.InvokeMember(
                    CSharpBinderFlags.None,
                    membername,
                    null,
                    type,
                    expArgs.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
                    );

                return Expression.Dynamic(binderM, typeof(object), expArgs);
            }
            else
            {
                var mis = MethodResolution.GetApplicableMembers(type, membername, args);
                var methodInfo = (MethodInfo)mis[0];

                var returnTypeArgs = methodInfo.GetGenericArguments();

                Dictionary<string, Type> genericArgTypes = null;

                if (methodInfo.IsGenericMethod)
                {
                    genericArgTypes = returnTypeArgs.ToDictionary(t => t.Name, null);
                }

                // if the method is generic, try to get type args from method, if none, try to get type args from parameters

                if (methodInfo != null)
                {
                    var parameterInfos = methodInfo.GetParameters();

                    foreach (var parameterInfo in parameterInfos)
                    {
                        var index = parameterInfo.Position;

                        if (parameterInfo.ParameterType.IsGenericType)
                        {
                            if (methodInfo.IsGenericMethod && parameterInfo.ParameterType.IsGenericParameter &&
                                genericArgTypes != null)
                            {
                                genericArgTypes[parameterInfo.Name] = args[index].Type;

                                //genericArgTypes[parameterInfo.ParameterType.GenericParameterPosition] = args[index].Type;
                                args[index] = Expression.Convert(args[index],
                                                                 parameterInfos[index].ParameterType
                                                                                      .GetGenericTypeDefinition()
                                                                                      .MakeGenericType(args[index].Type));
                            }
                            if (methodInfo.IsGenericMethod && parameterInfo.ParameterType.IsGenericType &&
                                genericArgTypes != null)
                            {

                                foreach (var pInfoGenericArgType in parameterInfo.ParameterType.GetGenericArguments())
                                {
                                    if (!genericArgTypes.ContainsKey(pInfoGenericArgType.Name))
                                    {
                                        genericArgTypes[pInfoGenericArgType.Name] = args[index].Type.GetGenericArguments()[0];
                                    }
                                    else
                                    {
                                        if (genericArgTypes[pInfoGenericArgType.Name] == null)
                                        {
                                            genericArgTypes[pInfoGenericArgType.Name] = args[index].Type.GetGenericArguments()[0];

                                        }
                                    }
                                    //genericArgTypes[parameterInfo.Position] =
                                    //    args[index].Type.GetGenericArguments()[0] ?? typeof(string);
                                }
                                //args[index] = Expression.Convert(args[index],
                                //                                 parameterInfos[index].ParameterType
                                //                                                      .GetGenericTypeDefinition()
                                //                                                      .MakeGenericType(typeof(string)));
                            }
                        }
                        else
                        {
                            if (methodInfo.IsGenericMethod && parameterInfo.ParameterType.IsGenericParameter &&
                                genericArgTypes != null)
                            {
                                genericArgTypes[parameterInfo.Name] = args[index].Type;
                                //genericArgTypes[parameterInfo.ParameterType.GenericParameterPosition] = args[index].Type;
                            }
                            args[index] = TypeConversion.Convert(args[index], parameterInfo.ParameterType);
                        }
                    }

                    if (isRuntimeType)
                    {
                        if (methodInfo.IsGenericMethod)
                        {
                            return Expression.Call(type, membername, genericArgTypes.Values.ToArray(), args.ToArray());
                        }
                        else
                        {
                            return Expression.Call(type, membername, null, args.ToArray());
                        }
                    }
                    else
                    {
                        if (methodInfo.IsGenericMethod)
                        {
                            return Expression.Call(instance, membername, genericArgTypes.Values.ToArray(), args.ToArray());
                        }
                        else
                        {
                            return Expression.Call(instance, methodInfo, args.ToArray());
                        }
                    }

                }


                var match = MethodResolution.GetExactMatch(type, instance, membername, args) ??
                            MethodResolution.GetParamsMatch(type, instance, membername, args);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        public static Expression ParseRealLiteral(string token)
        {
            var m = Regex.Match(token, "(\\d+(.\\d+)?)(d|f|m)?", RegexOptions.IgnoreCase);
            var suffix = "";

            Type ntype = null;
            object val = null;

            if (m.Success)
            {
                token = m.Groups[1].Value;

                if (m.Groups[3].Success)
                {
                    suffix = m.Groups[3].Value;
                }


                if (suffix.Length > 0)
                {
                    switch (suffix.ToLower())
                    {
                        case "d":
                            ntype = typeof(Double);
                            val = double.Parse(token, CultureInfo.InvariantCulture);
                            break;
                        case "f":
                            ntype = typeof(Single);
                            val = float.Parse(token, CultureInfo.InvariantCulture);
                            break;
                        case "m":
                            ntype = typeof(Decimal);
                            val = decimal.Parse(token, CultureInfo.InvariantCulture);
                            break;
                    }

                }
                else
                {
                    ntype = typeof(Double);
                    val = double.Parse(token, CultureInfo.InvariantCulture);
                }
            }
            return Expression.Constant(val, ntype);
        }


        public static Expression ParseIntLiteral(string token)
        {
            var m = Regex.Match(token, "(\\d+)(l|u|ul|lu)?", RegexOptions.IgnoreCase);
            string suffix = "";

            if (m.Success)
            {
                token = m.Groups[1].Value;

                if (m.Groups[2].Success)
                {
                    suffix = m.Groups[2].Value;
                }

                Type ntype = null;
                object val = null;

                if (suffix.Length > 0)
                {
                    switch (suffix.ToLower())
                    {
                        case "l":
                            ntype = typeof(Int64);
                            val = long.Parse(token, CultureInfo.InvariantCulture);
                            break;
                        case "u":
                            ntype = typeof(UInt32);
                            val = uint.Parse(token, CultureInfo.InvariantCulture);
                            break;
                        case "ul":
                        case "lu":
                            ntype = typeof(UInt64);
                            val = ulong.Parse(token, CultureInfo.InvariantCulture);
                            break;
                    }

                }
                else
                {
                    ntype = typeof(Int32);
                    val = int.Parse(token, CultureInfo.InvariantCulture);
                }
                return Expression.Constant(val, ntype);
            }
            throw new Exception("Invalid int literal");
        }

        public static Expression ParseDateLiteral(string token)
        {
            token = token.Substring(1, token.Length - 2);
            return Expression.Constant(DateTime.Parse(token));
        }

        public static Expression ParseHexLiteral(string token)
        {
            var m = Regex.Match(token, "(0[x][0-9|a-f]+)(l|u|ul|lu)?", RegexOptions.IgnoreCase);
            string suffix = "";

            if (m.Success)
            {
                token = m.Groups[1].Value;

                if (m.Groups[2].Success)
                {
                    suffix = m.Groups[2].Value;
                }

                Type ntype = typeof(Int32);
                object val = null;

                if (suffix.Length > 0)
                {
                    switch (suffix.ToLower())
                    {
                        case "l":
                            ntype = typeof(Int64);
                            val = Convert.ToInt64(token, 16);
                            break;
                        case "u":
                            ntype = typeof(UInt32);
                            val = Convert.ToUInt32(token, 16);
                            break;
                        case "ul":
                        case "lu":
                            ntype = typeof(UInt64);
                            val = Convert.ToUInt64(token, 16);
                            break;
                    }
                }
                else
                {
                    ntype = typeof(Int32);
                    val = Convert.ToInt32(token, 16);
                }

                return Expression.Constant(val, ntype);
            }
            return null;
        }


        public static Expression ParseCharLiteral(string token)
        {
            token = token.Substring(2, token.Length - 3);
            return Expression.Constant(Convert.ToChar(token), typeof(char));
        }

        public static Expression ParseStringLiteral(string token)
        {
            token = token.Substring(1, token.Length - 2);
            return Expression.Constant(token, typeof(string));
        }

        public static Expression UnaryOperator(Expression le, ExpressionType expressionType)
        {
            // perform implicit conversion on known types

            if (le.Type.IsDynamic())
            {
                return DynamicUnaryOperator(le, expressionType);
            }
            else
            {
                return GetUnaryOperator(le, expressionType);
            }
        }

        public static Expression BinaryOperator(Expression le, Expression re, ExpressionType expressionType)
        {
            // perform implicit conversion on known types

            if (le.Type.IsDynamic() && re.Type.IsDynamic())
            {
                if (expressionType == ExpressionType.OrElse)
                {
                    le = Expression.IsTrue(Expression.Convert(le, typeof(bool)));
                    expressionType = ExpressionType.Or;
                    return Expression.Condition(le, Expression.Constant(true), Expression.Convert(DynamicBinaryOperator(Expression.Constant(false), re, expressionType), typeof(bool)));
                }


                if (expressionType == ExpressionType.AndAlso)
                {
                    le = Expression.IsFalse(Expression.Convert(le, typeof(bool)));
                    expressionType = ExpressionType.And;
                    return Expression.Condition(le, Expression.Constant(false), Expression.Convert(DynamicBinaryOperator(Expression.Constant(true), re, expressionType), typeof(bool)));
                }

                return DynamicBinaryOperator(le, re, expressionType);
            }
            else
            {
                TypeConversion.Convert(ref le, ref re);

                return GetBinaryOperator(le, re, expressionType);
            }
        }

        public static Expression GetUnaryOperator(Expression le, ExpressionType expressionType)
        {
            switch (expressionType)
            {

                case ExpressionType.Negate:
                    return Expression.Negate(le);

                case ExpressionType.UnaryPlus:
                    return Expression.UnaryPlus(le);

                case ExpressionType.NegateChecked:
                    return Expression.NegateChecked(le);

                case ExpressionType.Not:
                    return Expression.Not(le);

                case ExpressionType.Decrement:
                    return Expression.Decrement(le);

                case ExpressionType.Increment:
                    return Expression.Increment(le);

                case ExpressionType.OnesComplement:
                    return Expression.OnesComplement(le);

                case ExpressionType.PreIncrementAssign:
                    return Expression.PreIncrementAssign(le);

                case ExpressionType.PreDecrementAssign:
                    return Expression.PreDecrementAssign(le);

                case ExpressionType.PostIncrementAssign:
                    return Expression.PostIncrementAssign(le);

                case ExpressionType.PostDecrementAssign:
                    return Expression.PostDecrementAssign(le);

                default:
                    throw new ArgumentOutOfRangeException("expressionType");
            }
        }

        public static Expression GetBinaryOperator(Expression le, Expression re, ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Add:
                    return Add(le, re);

                case ExpressionType.And:
                    return Expression.And(le, re);

                case ExpressionType.AndAlso:
                    return Expression.AndAlso(le, re);

                case ExpressionType.Coalesce:
                    return Expression.Coalesce(le, re);

                case ExpressionType.Divide:
                    return Expression.Divide(le, re);

                case ExpressionType.Equal:
                    return Expression.Equal(le, re);

                case ExpressionType.ExclusiveOr:
                    return Expression.ExclusiveOr(le, re);

                case ExpressionType.GreaterThan:
                    return Expression.GreaterThan(le, re);

                case ExpressionType.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(le, re);

                case ExpressionType.LeftShift:
                    return Expression.LeftShift(le, re);

                case ExpressionType.LessThan:
                    return Expression.LessThan(le, re);

                case ExpressionType.LessThanOrEqual:
                    return Expression.LessThanOrEqual(le, re);

                case ExpressionType.Modulo:
                    return Expression.Modulo(le, re);

                case ExpressionType.Multiply:
                    return Expression.Multiply(le, re);

                case ExpressionType.NotEqual:
                    return Expression.NotEqual(le, re);

                case ExpressionType.Or:
                    return Expression.Or(le, re);

                case ExpressionType.OrElse:
                    return Expression.OrElse(le, re);

                case ExpressionType.Power:
                    return Expression.Power(le, re);

                case ExpressionType.RightShift:
                    return Expression.RightShift(le, re);

                case ExpressionType.Subtract:
                    return Expression.Subtract(le, re);

                case ExpressionType.Assign:
                    return Expression.Assign(le, re);

                case ExpressionType.AddAssign:
                    return Expression.AddAssign(le, re);

                case ExpressionType.AndAssign:
                    return Expression.AndAssign(le, re);

                case ExpressionType.DivideAssign:
                    return Expression.DivideAssign(le, re);

                case ExpressionType.ExclusiveOrAssign:
                    return Expression.ExclusiveOrAssign(le, re);

                case ExpressionType.LeftShiftAssign:
                    return Expression.LeftShiftAssign(le, re);

                case ExpressionType.ModuloAssign:
                    return Expression.ModuloAssign(le, re);

                case ExpressionType.MultiplyAssign:
                    return Expression.MultiplyAssign(le, re);

                case ExpressionType.OrAssign:
                    return Expression.OrAssign(le, re);

                case ExpressionType.PowerAssign:
                    return Expression.PowerAssign(le, re);

                case ExpressionType.RightShiftAssign:
                    return Expression.RightShiftAssign(le, re);

                case ExpressionType.SubtractAssign:
                    return Expression.SubtractAssign(le, re);

                case ExpressionType.AddAssignChecked:
                    return Expression.AddAssignChecked(le, re);

                case ExpressionType.MultiplyAssignChecked:
                    return Expression.MultiplyAssignChecked(le, re);

                case ExpressionType.SubtractAssignChecked:
                    return Expression.SubtractAssignChecked(le, re);

                default:
                    throw new ArgumentOutOfRangeException("expressionType");
            }
        }

        private static Expression DynamicUnaryOperator(Expression le, ExpressionType expressionType)
        {
            var expArgs = new List<Expression>() { le };

            var binderM = Binder.UnaryOperation(CSharpBinderFlags.None, expressionType, le.Type, new CSharpArgumentInfo[]
		            {
			            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
			            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
		            });

            return Expression.Dynamic(binderM, typeof(object), expArgs);
        }

        private static Expression DynamicBinaryOperator(Expression le, Expression re, ExpressionType expressionType)
        {
            var expArgs = new List<Expression>() { le, re };


            var binderM = Binder.BinaryOperation(CSharpBinderFlags.None, expressionType, le.Type, new CSharpArgumentInfo[]
		            {
			            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
			            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
		            });

            return Expression.Dynamic(binderM, typeof(object), expArgs);
        }


        public static Expression Condition(Expression condition, Expression ifTrue, Expression ifFalse)
        {
            if (condition.Type != typeof(bool))
            {
                condition = Expression.Convert(condition, typeof(bool));
            }

            // perform implicit conversion on known types ???
            TypeConversion.Convert(ref ifFalse, ref ifTrue);
            return Expression.Condition(condition, ifTrue, ifFalse);
        }

        public static Expression New(Type t, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> memberInfos)
        {
            ConstructorInfo constructorInfo;

            if (arguments == null)
            {
                var p = t.GetConstructors();
                constructorInfo = p.First(x => !x.GetParameters().Any());
            }
            else
            {
                constructorInfo = t.GetConstructor(arguments.Select(arg => arg.Type).ToArray());
            }


            if (memberInfos == null)
            {
                return Expression.New(constructorInfo, arguments);
            }
            else
            {
                return Expression.New(constructorInfo, arguments, memberInfos);
            }
        }

    }
}