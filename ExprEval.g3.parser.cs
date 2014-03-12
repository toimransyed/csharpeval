using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Antlr.Runtime;
using ExpressionEvaluator.Operators;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace ExpressionEvaluator
{
    public class UnknownIdentifierException : Exception
    {
        
    }

    public class UnknownMethodException : Exception
    {

    }

    public partial class ExprEvalParser
    {
        public Expression Scope { get; set; }
        public bool IsCall { get; set; }

        public Dictionary<string, object> TypeRegistry { get; set; }

        public override void ReportError(RecognitionException e)
        {
            base.ReportError(e);
            Console.WriteLine("Error in parser at line " + e.Line + ":" + e.CharPositionInLine);
        }

        private Expression GetPropertyIndex(Expression le, IEnumerable<Expression> args)
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

        private Expression GetProperty(Expression le, string membername)
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

        private Expression GetMethod(Expression le, string membername, List<Expression> args)
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
                var expArgs = new List<Expression> { instance };

                expArgs.AddRange(args);

                if (IsCall)
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

                var typeArgs = methodInfo.GetGenericArguments();

                Type[] genericArgTypes = null;

                if (methodInfo.IsGenericMethod)
                {
                    genericArgTypes = new Type[typeArgs.Count()];
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
                                genericArgTypes[parameterInfo.ParameterType.GenericParameterPosition] = args[index].Type;
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
                                    genericArgTypes[pInfoGenericArgType.GenericParameterPosition] =
                                        args[index].Type.GetElementType() ?? typeof(string);
                                }
                                args[index] = Expression.Convert(args[index],
                                                                 parameterInfos[index].ParameterType
                                                                                      .GetGenericTypeDefinition()
                                                                                      .MakeGenericType(typeof(string)));
                            }
                        }
                        else
                        {
                            if (methodInfo.IsGenericMethod && parameterInfo.ParameterType.IsGenericParameter &&
                                genericArgTypes != null)
                            {
                                genericArgTypes[parameterInfo.ParameterType.GenericParameterPosition] = args[index].Type;
                            }
                            args[index] = TypeConversion.Convert(args[index], parameterInfo.ParameterType);
                        }
                    }

                    if (isRuntimeType)
                    {
                        if (methodInfo.IsGenericMethod)
                        {
                            return Expression.Call(type, membername, genericArgTypes, args.ToArray());
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
                            return Expression.Call(instance, membername, genericArgTypes, args.ToArray());
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

            throw new UnknownMethodException();
        }

        private Expression GetIdentifier(string identifier)
        {
            object result = null;
            if (TypeRegistry.TryGetValue(identifier, out result))
            {
                return Expression.Constant(result);
            }
            return null;

            throw new UnknownIdentifierException();
        }

        private Expression ParseRealLiteral(string token, string exponent, string suffix)
        {
            Type ntype = null;
            object val = null;

            if (suffix.Length > 0)
            {
                switch (suffix.ToLower())
                {
                    case "d":
                        ntype = typeof(Double);
                        val = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "f":
                        ntype = typeof(Single);
                        val = float.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "m":
                        ntype = typeof(Decimal);
                        val = decimal.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                }

            }
            else
            {
                ntype = typeof(Double);
                val = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
            }
            return Expression.Constant(val, ntype);
        }

        private Expression ParseRealLiteral(string token)
        {
            var m = Regex.Match(token, "(\\d+(.\\d+)?)(d|f|m)?", RegexOptions.IgnoreCase);
            string suffix = "";

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
                            val = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "f":
                            ntype = typeof(Single);
                            val = float.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "m":
                            ntype = typeof(Decimal);
                            val = decimal.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                    }

                }
                else
                {
                    ntype = typeof(Double);
                    val = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return Expression.Constant(val, ntype);
        }


        private Expression ParseIntLiteral(string token)
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
                            val = long.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "u":
                            ntype = typeof(UInt32);
                            val = uint.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ul":
                        case "lu":
                            ntype = typeof(UInt64);
                            val = ulong.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                    }

                }
                else
                {
                    ntype = typeof(Int32);
                    val = int.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                }
                return Expression.Constant(val, ntype);
            }
            throw new Exception("Invalid int literal");
        }
    }
}
