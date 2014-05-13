using System;
using ExpressionEvaluator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class ImplicitConversions
    {

        [TestMethod]
        public void ReferenceConversions()
        {
            var refType = new ReferenceType();
            var derRef = new DerivedReferenceType();
            var objHolder = new ObjHolder();
            var t = new TypeRegistry();
            t.RegisterSymbol("r", refType);
            t.RegisterSymbol("d", derRef);
            t.RegisterSymbol("o", objHolder);
            var c = new CompiledExpression() { TypeRegistry = t };

            //•	From any reference-type to object and dynamic.
            c.StringToParse = "o.ObjContainer = r";
            c.Eval();
            Assert.AreEqual(objHolder.ObjContainer, refType);

            //•	From any class-type S to any class-type T, provided S is derived from T.
            c.StringToParse = "o.RefContainer = d";
            c.Eval();
            Assert.AreEqual(objHolder.RefContainer, derRef);

            //•	From any class-type S to any interface-type T, provided S implements T.
            c.StringToParse = "o.IRefContainer = r";
            c.Eval();
            Assert.AreEqual(objHolder.IRefContainer, refType);

            c.StringToParse = "o.IRefContainer = d";
            c.Eval();
            Assert.AreEqual(objHolder.IRefContainer, derRef);

            c.StringToParse = "o.IRefContainer2 = d";
            c.Eval();
            Assert.AreEqual(objHolder.IRefContainer2, derRef);

            //•	From any interface-type S to any interface-type T, provided S is derived from T.
            c.StringToParse = "o.IRefContainer = o.IRefContainer2";
            c.Eval();
            Assert.AreEqual(objHolder.IRefContainer, objHolder.IRefContainer2);
        }
    }

    public interface IReferenceType
    {
        object Test { get; set; }
    }

    public interface IReferenceType2 : IReferenceType
    {
    }

    public class ReferenceType : IReferenceType
    {
        public object Test { get; set; }
    }

    public class DerivedReferenceType : ReferenceType, IReferenceType2
    {

    }

    public class ObjHolder
    {
        public object ObjContainer { get; set; }
        public ReferenceType RefContainer { get; set; }
        public IReferenceType IRefContainer { get; set; }
        public IReferenceType2 IRefContainer2 { get; set; }
    }
}
