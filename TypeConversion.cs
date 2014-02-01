using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionEvaluator
{
    class TypeConversion
    {
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
        }
    }
}