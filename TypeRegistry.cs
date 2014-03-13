using System;
using System.Collections.Generic;


namespace ExpressionEvaluator
{
    public class TypeRegistry : Dictionary<string, object>
    {
        public TypeRegistry()
        {
            //Add default types
            Add("object", typeof(Object));
            Add("bool", typeof(Boolean));
            Add("byte", typeof(Byte));
            Add("char", typeof(Char));
            Add("short", typeof(Int16));
            Add("int", typeof(Int32));
            Add("long", typeof(Int64));
            Add("ushort", typeof(UInt16));
            Add("uint", typeof(UInt32));
            Add("ulong", typeof(UInt64));
            Add("decimal", typeof(Decimal));
            Add("double", typeof(Double));
            Add("float", typeof(Single));
            Add("string", typeof(String));

            Add("Object", typeof(Object));
            Add("Boolean", typeof(Boolean));
            Add("Byte", typeof(Byte));
            Add("Char", typeof(Char));
            Add("Int16", typeof(Int16));
            Add("Int32", typeof(Int32));
            Add("Int64", typeof(Int64));
            Add("UInt16", typeof(UInt16));
            Add("UInt32", typeof(UInt32));
            Add("UInt64", typeof(UInt64));
            Add("Decimal", typeof(Decimal));
            Add("Double", typeof(Double));
            Add("Single", typeof(Single));
            Add("String", typeof(String));
        }

        public void RegisterDefaultTypes()
        {
            Add("DateTime", typeof(DateTime));
            Add("Convert", typeof(Convert));
            Add("Math", typeof(Math));
        }

        public void RegisterType<T>()
        {
            var t = typeof(T);
            Add(t.Name, t);
        }

        public void RegisterType<T>(string alias)
        {
            var t = typeof(T);
            Add(alias, t);
        }

        public void RegisterType(string alias, Type t)
        {
            Add(alias, t);
        }

        public void RegisterSymbol(string identifier, object value)
        {
            Add(identifier, value);
        }
    }

}
