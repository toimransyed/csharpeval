using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace ExpressionEvaluator.Operators
{
    internal class OperatorCustomExpressions
    {
        /// <summary>
        /// Returns an Expression that accesses a member on an Expression
        /// </summary>
        /// <param name="isFunction">Determines whether the member being accessed is a function or a property</param>
        /// <param name="le">The expression that contains the member to be accessed</param>
        /// <param name="membername">The name of the member to access</param>
        /// <param name="args">Optional list of arguments to be passed if the member is a method</param>
        /// <returns></returns>
        public static Expression MemberAccess(bool isFunction, Expression le, string membername, List<Expression> args)
        {
            var argTypes = args.Select(x => x.Type);

            Expression instance = null;
            Type type = null;

            var isDynamic = false;

            if (le.Type.Name == "RuntimeType")
            {
                type = ((Type)((ConstantExpression)le).Value);
            }
            else
            {
                type = le.Type;
                instance = le;
                isDynamic = type.IsDynamic();
            }

            if (isFunction)
            {
                if (isDynamic)
                {
                    var expArgs = new List<Expression> { instance };

                    expArgs.AddRange(args);

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
                    // TODO: Test overloaded methods with different types

                    // Look for an exact match
                    var methodInfo = type.GetMethod(membername, argTypes.ToArray());

                    if (methodInfo != null)
                    {
                        var parameterInfos = methodInfo.GetParameters();

                        for (int i = 0; i < parameterInfos.Length; i++)
                        {
                            args[i] = TypeConversion.Convert(args[i], parameterInfos[i].ParameterType);
                        }

                        return Expression.Call(instance, methodInfo, args);
                    }

                    // assume params

                    var methodInfos = type.GetMethods().Where(x => x.Name == membername);
                    var matchScore = new List<Tuple<MethodInfo, int>>();

                    foreach (var info in methodInfos.OrderByDescending(m => m.GetParameters().Count()))
                    {
                        var parameterInfos = info.GetParameters();
                        var lastParam = parameterInfos.Last();
                        var newArgs = args.Take(parameterInfos.Length - 1).ToList();
                        var paramArgs = args.Skip(parameterInfos.Length - 1).ToList();

                        int i = 0;
                        int k = 0;

                        foreach (var expression in newArgs)
                        {
                            k += TypeConversion.CanConvert(expression.Type, parameterInfos[i].ParameterType);
                            i++;
                        }

                        if (k > 0)
                        {
                            if (Attribute.IsDefined(lastParam, typeof(ParamArrayAttribute)))
                            {
                                k += paramArgs.Sum(arg => TypeConversion.CanConvert(arg.Type, lastParam.ParameterType.GetElementType()));
                            }
                        }

                        matchScore.Add(new Tuple<MethodInfo, int>(info, k));
                    }

                    var info2 = matchScore.OrderBy(x => x.Item2).FirstOrDefault(x => x.Item2 >= 0);
                    if (info2 != null)
                    {
                        var parameterInfos2 = info2.Item1.GetParameters();
                        var lastParam2 = parameterInfos2.Last();
                        var newArgs2 = args.Take(parameterInfos2.Length - 1).ToList();
                        var paramArgs2 = args.Skip(parameterInfos2.Length - 1).ToList();


                        for (int i = 0; i < parameterInfos2.Length - 1; i++)
                        {
                            newArgs2[i] = TypeConversion.Convert(newArgs2[i], parameterInfos2[i].ParameterType);
                        }

                        var targetType = lastParam2.ParameterType.GetElementType();

                        newArgs2.Add(Expression.NewArrayInit(targetType, paramArgs2.Select(x => TypeConversion.Convert(x, targetType))));
                        return Expression.Call(instance, info2.Item1, newArgs2);
                    }

                }

            }
            else
            {
                if (isDynamic)
                {
                    var binder = Binder.GetMember(
                        CSharpBinderFlags.None,
                        membername,
                        type,
                        new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }
                        );

                    var result = Expression.Dynamic(binder, typeof(object), instance);


                    if (args.Count > 0)
                    {
                        var expArgs = new List<Expression>() { result };

                        expArgs.AddRange(args);

                        var indexedBinder = Binder.GetIndex(
                            CSharpBinderFlags.None,
                            type,
                            expArgs.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
                            );

                        result =
                            Expression.Dynamic(indexedBinder, typeof(object), expArgs);

                    }

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

                    if (exp != null)
                    {
                        if (args.Count > 0)
                        {
                            return Expression.ArrayAccess(exp, args);
                        }
                        else
                        {
                            return exp;
                        }
                    }
                }


            }

            throw new Exception(string.Format("Member not found: {0}.{1}", le.Type.Name, membername));
        }



        /// <summary>
        /// Extends the Add Expression handler to handle string concatenation
        /// </summary>
        /// <param name="le">The left-hand expression</param>
        /// <param name="re">The right-hand expression</param>
        /// <returns></returns>
        public static Expression Add(Expression le, Expression re)
        {
            if (le.Type == typeof(string) || re.Type == typeof(string))
            {
                return Expression.Add(le, re, typeof(string).GetMethod("Concat", new Type[] { le.Type, re.Type }));
            }
            else
            {
                return Expression.Add(le, re);
            }
        }

        private static Type _stringType = typeof(string);

        /// <summary>
        /// Returns an Expression that access a 1-dimensional index on an Array expression 
        /// </summary>
        /// <param name="le">The left-hand expression</param>
        /// <param name="re">The right-hand expression</param>
        /// <returns></returns>
        public static Expression ArrayAccess(Expression le, Expression re)
        {
            if (le.Type == _stringType)
            {
                var mi = _stringType.GetMethod("ToCharArray", new Type[] { });
                le = Expression.Call(le, mi);
            }

            return Expression.ArrayAccess(le, re);
        }

        /// <summary>
        /// Placeholderthat simple returns the left expression
        /// </summary>
        /// <param name="le"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static Expression TernarySeparator(Expression le)
        {
            return le;
        }

    }
}