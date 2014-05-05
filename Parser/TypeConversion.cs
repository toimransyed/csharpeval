using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator.Parser
{
    internal class TypeConversion
    {
        private static Dictionary<Type, List<Type>> ImplicitNumericConversions = new Dictionary<Type, List<Type>>();
        private static List<Type> NumericTypes = new List<Type>();

        //public static void ConvertType(ExpressionType expressionType, ref Expression a, ref Expression b, Type type, Type[] otherTypes, Type[] invalidTypes, Type targetType)
        //{
        //    if (targetType == null) targetType = type;

        //    if (a.Type == type && b.Type != type)
        //    {
        //        if (otherTypes != null)
        //        {
        //            if (!otherTypes.Contains(b.Type)) return;
        //        }
        //        if (invalidTypes != null)
        //        {
        //            if (invalidTypes.Contains(b.Type))
        //            {
        //                // 
        //                throw new Exception(string.Format("Cannot apply operator {0} to operands of type {1} and {2}", expressionType, a.Type, b.Type));
        //            }
        //        }
        //        b = Expression.Convert(b, targetType);
        //    }
        //}

        //public static bool ConvertTypes(ExpressionType expressionType, ref Expression le, ref Expression re, Type type, Type[] otherTypes = null, Type[] invalidTypes = null, Type targetType = null)
        //{
        //    if (le.Type == type || re.Type == type)
        //    {
        //        ConvertType(expressionType, ref le, ref re, type, otherTypes, invalidTypes, targetType);
        //        ConvertType(expressionType, ref re, ref le, type, otherTypes, invalidTypes, targetType);
        //        return true;
        //    }
        //    return false;
        //}

        //public static void PromoteNumericBinary(ExpressionType expressionType, ref Expression le, ref Expression re)
        //{
        //    // 7.3.6.2 
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(decimal), null, new Type[] { typeof(float), typeof(double) })) return;
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(double))) return;
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(float))) return;
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(ulong), null, new Type[] { typeof(sbyte), typeof(short), typeof(int), typeof(long) })) return;
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(long))) return;
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(uint), new Type[] { typeof(sbyte), typeof(short), typeof(int) }, null, typeof(long))) return;
        //    if (ConvertTypes(expressionType, ref le, ref re, typeof(uint))) return;
        //    ConvertTypes(expressionType, ref le, ref re, typeof(int));
        //}

        readonly Dictionary<Type, int> _typePrecedence = null;
        static readonly TypeConversion Instance = new TypeConversion();
        /// <summary>
        /// Performs implicit conversion between two expressions depending on their type precedence
        /// </summary>
        /// <param name="le"></param>
        /// <param name="re"></param>
        internal static void Convert(ref Expression le, ref Expression re)
        {
            if (Instance._typePrecedence.ContainsKey(le.Type) && Instance._typePrecedence.ContainsKey(re.Type))
            {
                if (Instance._typePrecedence[le.Type] > Instance._typePrecedence[re.Type]) re = Expression.Convert(re, le.Type);
                if (Instance._typePrecedence[le.Type] < Instance._typePrecedence[re.Type]) le = Expression.Convert(le, re.Type);
            }
        }

        /// <summary>
        /// Performs implicit conversion on an expression against a specified type
        /// </summary>
        /// <param name="le"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Expression Convert(Expression le, Type type)
        {
            if (Instance._typePrecedence.ContainsKey(le.Type) && Instance._typePrecedence.ContainsKey(type))
            {
                if (Instance._typePrecedence[le.Type] < Instance._typePrecedence[type]) return Expression.Convert(le, type);
            }
            return le;
        }

        /// <summary>
        /// Compares two types for implicit conversion
        /// </summary>
        /// <param name="from">The source type</param>
        /// <param name="to">The destination type</param>
        /// <returns>-1 if conversion is not possible, 0 if no conversion necessary, +1 if conversion possible</returns>
        internal static int CanConvert(Type from, Type to)
        {
            if (Instance._typePrecedence.ContainsKey(from) && Instance._typePrecedence.ContainsKey(to))
            {
                return Instance._typePrecedence[to] - Instance._typePrecedence[from];
            }
            else
            {
                if (from == to) return 0;
                if (to.IsAssignableFrom(from)) return 1;
            }
            return -1;
        }

        // 6.1.7 Boxing Conversions
        // A boxing conversion permits a value-type to be implicitly converted to a reference type. A boxing conversion exists from any non-nullable-value-type to object and dynamic, to System.ValueType and to any interface-type implemented by the non-nullable-value-type. Furthermore an enum-type can be converted to the type System.Enum.
        // A boxing conversion exists from a nullable-type to a reference type, if and only if a boxing conversion exists from the underlying non-nullable-value-type to the reference type.
        // A value type has a boxing conversion to an interface type I if it has a boxing conversion to an interface type I0 and I0 has an identity conversion to I.

        public static Expression BoxingConversion(Expression dest, Expression src)
        {
            if (src.Type.IsValueType && dest.Type.IsDynamicOrObject())
            {
                src = Expression.Convert(src, dest.Type);
            }
            return src;
        }

        public static Expression ImplicitConversion(Expression dest, Expression src)
        {
            if (dest.Type != src.Type)
            {
                if (IsNumericType(dest.Type) && IsNumericType(src.Type))
                {
                    src = ImplicitNumericConversion(src, dest.Type);
                }
                src = BoxingConversion(dest, src);
            }
            return src;
        }

        // 6.1.2 Implicit numeric conversions

        public static Expression ImplicitNumericConversion(Expression src, Type target)
        {
            List<Type> allowed;
            if (ImplicitNumericConversions.TryGetValue(src.Type, out allowed))
            {
                if (allowed.Contains(target))
                {
                    src = Expression.Convert(src, target);
                }
            }
            return src;
        }

        public static bool IsNumericType(Type t)
        {
            return NumericTypes.Contains(t);
        }

        TypeConversion()
        {
            _typePrecedence = new Dictionary<Type, int>
                {
                    {typeof (object), 0},
                    {typeof (bool), 1},
                    {typeof (byte), 2},
                    {typeof (int), 3},
                    {typeof (short), 4},
                    {typeof (long), 5},
                    {typeof (float), 6},
                    {typeof (double), 7}
                };

            NumericTypes = new List<Type>()
                {
                    typeof (sbyte),
                    typeof (byte),
                    typeof (short),
                    typeof (ushort),
                    typeof (int),
                    typeof (uint),
                    typeof (long),
                    typeof (ulong),
                    typeof (char),
                    typeof (float)
                };

            ImplicitNumericConversions.Add(typeof(sbyte), new List<Type>() { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(byte), new List<Type>() { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(short), new List<Type>() { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(ushort), new List<Type>() { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(int), new List<Type>() { typeof(long), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(uint), new List<Type>() { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(long), new List<Type>() { typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(ulong), new List<Type>() { typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(char), new List<Type>() { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
            ImplicitNumericConversions.Add(typeof(float), new List<Type>() { typeof(double) });
        }
    }
}