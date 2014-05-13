using System;
using System.Dynamic;
using System.Linq;

namespace ExpressionEvaluator.Parser
{
    internal static class TypeExtensions
    {
        public static bool IsDynamicOrObject(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider)) ||
                   type == typeof(Object);
        }

        public static bool IsDynamic(this Type type)
        {
            return type.GetInterfaces().Contains(typeof (IDynamicMetaObjectProvider));
        }

        public static bool IsObject(this Type type)
        {
            return type == typeof(Object);
        }
    }

    //internal static class ExpressionExtensions
    //{
    //    public static bool IsDynamicOrObject(this Type type)
    //    {
    //        return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider)) ||
    //               type == typeof(Object);
    //    }

    //    public static bool IsDynamic(this Expression type)
    //    {
    //        return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider));
    //    }

    //    public static bool IsObject(this Type type)
    //    {
    //        return type == typeof(Object);
    //    }
    //}
}