using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionEvaluator
{
    public class PrimaryExpressionPartToken
    {

    }


    public class AccessIdentifierToken : PrimaryExpressionPartToken
    {
        public TypeOrGeneric Value { get; set; }
    }


    public class ExpressionListToken : PrimaryExpressionPartToken
    {
        public List<Expression> Values { get; set; }
    }

    public class BracketsToken : ExpressionListToken
    {
    }

    public class ArgumentsToken : ExpressionListToken
    {
    }

    public class PostIncrementToken : PrimaryExpressionPartToken
    {

    }

    public class PostDecrementToken : PrimaryExpressionPartToken
    {

    }
}
