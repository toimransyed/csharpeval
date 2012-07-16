using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionEvaluator
{
    internal class Token
    {
        public object value;
        public bool isIdent;
        public bool isOperator;
        public bool isVariable;
        public bool isType;
        public Type type;
    }

    internal class Operator
    {
        public string value;
        public int precedence;
        public int arguments;
        public bool leftassoc;
        public Func<Expression, Token, MethodCallExpression> mFunc;
        public Func<Expression, Expression, Expression> bFunc;
        public Func<Expression, UnaryExpression> uFunc;
        public Func<Expression, Type, UnaryExpression> tFunc;


        public Operator(string value, int precedence, int arguments, bool leftassoc)
        {
            this.value = value;
            this.precedence = precedence;
            this.arguments = arguments;
            this.leftassoc = leftassoc;
        }

        public Operator(string value, int precedence, int arguments, bool leftassoc, Func<Expression, Expression, Expression> bfunc)
        {
            this.value = value;
            this.precedence = precedence;
            this.arguments = arguments;
            this.leftassoc = leftassoc;
            this.bFunc = bfunc;
        }

        public Operator(string value, int precedence, int arguments, bool leftassoc, Func<Expression, UnaryExpression> ufunc)
        {
            this.value = value;
            this.precedence = precedence;
            this.arguments = arguments;
            this.leftassoc = leftassoc;
            this.uFunc = ufunc;
        }

        public Operator(string value, int precedence, int arguments, bool leftassoc, Func<Expression, Type, UnaryExpression> tfunc)
        {
            this.value = value;
            this.precedence = precedence;
            this.arguments = arguments;
            this.leftassoc = leftassoc;
            this.tFunc = tfunc;
        }

        public Operator(string value, int precedence, int arguments, bool leftassoc, Func<Expression, Token, MethodCallExpression> mfunc)
        {
            this.value = value;
            this.precedence = precedence;
            this.arguments = arguments;
            this.leftassoc = leftassoc;
            this.mFunc = mfunc;
        }

    }

    internal class OperatorCollection : Dictionary<string, Operator>
    {
        List<char> firstlookup = new List<char>();

        public new void Add(string key, Operator op)
        {
            firstlookup.Add(key[0]);
            base.Add(key, op);
        }

        public bool ContainsFirstKey(char key)
        {
            return firstlookup.Contains(key);
        }
    }

    internal class OpToken
    {
        public string value;
        public Type type;
    }

#if TYPE_SAFE
    public class Parser<T>
    {
#else
    public class Parser
    {
#endif
        string pstr;
        int ptr = 0;
        public object StateBag { get; set; }
        Queue<Token> tokenQueue = new Queue<Token>();
        Stack<OpToken> opStack = new Stack<OpToken>();
        static OperatorCollection operators;

#if TYPE_SAFE
        Func<T> compiled = null;
#else
        delegate object compiledFunc();
        compiledFunc compiled = null;
#endif

        public string StringToParse { get { return pstr; } set { pstr = value; compiled = null; tokenQueue.Clear(); } }

        public Parser()
        {
            Initialize();
        }

        public Parser(string str)
        {
            Initialize();
            pstr = str;
        }


        static void Initialize()
        {
            operators = new OperatorCollection();

            operators.Add(".", new Operator(".", 7, 1, true,
                delegate(Expression le, Token token)
                {
                    string s = (string)token.value;
                    MethodInfo mi = le.Type.GetMethod(s.Substring(0, s.IndexOf('(')));
                    return Expression.Call(le, mi);
                }
                ));


            operators.Add("!", new Operator("!", 6, 1, false, Expression.Not));
            operators.Add("^", new Operator("^", 6, 2, false, Expression.Power));
            operators.Add("*", new Operator("*", 5, 2, true, Expression.Multiply));
            operators.Add("/", new Operator("/", 5, 2, true, Expression.Divide));
            operators.Add("%", new Operator("%", 5, 2, true, Expression.Modulo));
            operators.Add("+", new Operator("+", 4, 2, true,
                delegate(Expression le, Expression re)
                {
                    if (le.Type == typeof(string) && re.Type == typeof(string))
                    {
                        return Expression.Add(le, re, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
                    }
                    else
                    {
                        return Expression.Add(le, re);
                    }
                }
                ));
            operators.Add("-", new Operator("-", 4, 2, true, Expression.Subtract));
            operators.Add("==", new Operator("==", 3, 2, true, Expression.Equal));
            operators.Add("!=", new Operator("!=", 3, 2, true, Expression.NotEqual));
            operators.Add("<", new Operator("<", 3, 2, true, Expression.LessThan));
            operators.Add(">", new Operator(">", 3, 2, true, Expression.GreaterThan));
            operators.Add("<=", new Operator("<=", 3, 2, true, Expression.LessThanOrEqual));
            operators.Add(">=", new Operator(">=", 3, 2, true, Expression.GreaterThanOrEqual));
            operators.Add("&&", new Operator("&&", 2, 2, true, Expression.And));
            operators.Add("||", new Operator("||", 1, 2, true, Expression.Or));


            operators.Add("[", new Operator("[", 0, 2, true,
                delegate(Expression le, Expression re)
                {
                    return Expression.ArrayAccess(le, re);
                }
                ));

        }

        static bool is_operator(string c)
        {
            int i = 0;
            return is_operator(c, ref i) != null;
        }

        // operators are of variable length, so we need to test at the current 
        // position for multiple lengths
        static string is_operator(string str, ref int p)
        {
            string op = null, pop = null;

            // Check id the first char in the string at the current position is 
            // part of an operator... (slight speedup? maybe not)

            if (operators.ContainsFirstKey(str[p]))
            {
                if (str.Substring(p).Length > 1)
                {
                    pop = str.Substring(p, 2);
                    if (operators.ContainsKey(pop))
                    {
                        p++;
                        op = pop;
                    }
                }
                if (op == null)
                {
                    pop = str.Substring(p, 1);
                    if (operators.ContainsKey(pop))
                        op = pop;
                }
            }

            // return the operator we found, or null otherwise
            return op;
        }


        static bool isaNumber(string str, int ptr)
        {
            return 
                ((str[ptr] == '-') && isNumeric(str, ptr+1)) || 
                isNumeric(str, ptr);
        }

        static bool isNumeric(string str, int ptr)
        {
            return (str[ptr] >= '0' && str[ptr] <= '9');
        }


        static bool isAlpha(char chr)
        {
            return (chr >= 'A' && chr <= 'Z') || (chr >= 'a' && chr <= 'z');
        }


        public void Parse()
        {
            try
            {
                tokenQueue.Clear();
                while (IsInBounds())
                {
                    string op = "";

                    int lastptr = ptr;

                    if (pstr[ptr] != ' ')
                    {
                        // Parse enclosed strings
                        if (pstr[ptr] == '\'')
                        {
                            ptr++;
                            bool escape = false;
                            lastptr = ptr;
                            StringBuilder tokenbuilder = new StringBuilder();

                            // check for escaped single-quote
                            while (IsInBounds())
                            {
                                if (escape)
                                {
                                    tokenbuilder.Append(pstr.Substring(ptr, 1));
                                    ptr++;
                                    lastptr = ptr;
                                    escape = false;
                                }
                                else if (pstr[ptr] == '\\')
                                {
                                    escape = true;
                                    tokenbuilder.Append(pstr.Substring(lastptr, ptr - lastptr));
                                    ptr++;
                                    lastptr = ptr;
                                }
                                else if ((pstr[ptr] == '\'') && !escape)
                                {
                                    break;
                                }
                                else
                                {
                                    ptr++;
                                }
                            }

                            tokenbuilder.Append(pstr.Substring(lastptr, ptr - lastptr));
                            string token = tokenbuilder.ToString();
                            tokenQueue.Enqueue(new Token() { value = token, isIdent = true, type = typeof(string) });

                            ptr++;
                        }
                        // Parse enclosed dates
                        else if (pstr[ptr] == '#')
                        {
                            ptr++;
                            lastptr = ptr;

                            while (IsInBounds())
                            {
                                ptr++;
                                if (pstr[ptr] == '#')
                                {
                                    break;
                                }
                            }

                            string token = pstr.Substring(lastptr, ptr - lastptr);

                            DateTime dt;
                            if (token == "Now")
                            {
                                dt = DateTime.Now;
                            }
                            else
                            {
                                dt = DateTime.Parse(token);
                            }

                            tokenQueue.Enqueue(new Token() { value = dt, isIdent = true, type = typeof(DateTime) });

                            ptr++;
                        }
                        // Parse numbers
                        else if (isaNumber(pstr, ptr))
                        {
                            // Number identifiers start with a number and may contain numbers and decimals
                            while (IsInBounds() && (isaNumber(pstr, ptr) || pstr[ptr] == '.' || pstr[ptr] == 'd' || pstr[ptr] == 'f'))
                            {
                                ptr++;
                            }

                            string token = pstr.Substring(lastptr, ptr - lastptr);

                            Type ntype = typeof(System.Int32);
                            object val = null;

                            if (token.Contains('.')) ntype = typeof(System.Double);
                            if (token.EndsWith("d") || token.EndsWith("f"))
                            {
                                if (token.EndsWith("d")) ntype = typeof(System.Double);
                                if (token.EndsWith("f")) ntype = typeof(System.Single);
                                token = token.Remove(token.Length - 1, 1);
                            }

                            switch (ntype.Name)
                            {
                                case "Int32":
                                    val = int.Parse(token);
                                    break;
                                case "Double":
                                    val = double.Parse(token);
                                    break;
                                case "Single":
                                    val = float.Parse(token);
                                    break;
                            }


                            tokenQueue.Enqueue(new Token() { value = val, isIdent = true, type = ntype });
                        }
                        else if (isAlpha(pstr[ptr]))
                        {
                            // Alphanumeric identifiers start with a letter and may contain letter, numbers and decimals 
                            while (IsInBounds() && (isAlpha(pstr[ptr]) || isNumeric(pstr,ptr) || pstr[ptr] == '.'))
                            {
                                ptr++;
                            }

                            string token = pstr.Substring(lastptr, ptr - lastptr);

                            //Type test = Type.GetType(token);
                            //if (test != null)
                            //{
                            //    op = "ty";
                            //    while (opStack.Count > 0)
                            //    {
                            //        OpToken sc = opStack.Peek();

                            //        if (is_operator(sc.value) &&
                            //             ((operators[op].leftassoc &&
                            //               (operators[op].precedence <= operators[sc.value].precedence)) ||
                            //               (operators[op].precedence < operators[sc.value].precedence))
                            //            )
                            //        {
                            //            OpToken popToken = opStack.Pop();
                            //            tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type });
                            //        }
                            //        else
                            //        {
                            //            break;
                            //        }
                            //    }
                            //    opStack.Push(new OpToken() { value = op, type = test });
                            //}
                            //else
                            //{
                            // Boolean identifiers
                            if ((token.ToLower() == "null"))
                            {
                                tokenQueue.Enqueue(new Token() { value = null, isIdent = true, type = typeof(object) });
                            }
                            else if ((token.ToLower() == "true") || (token.ToLower() == "false"))
                            {
                                tokenQueue.Enqueue(new Token() { value = Boolean.Parse(token), isIdent = true, type = typeof(Boolean) });
                            }
                            else
                            {
                                tokenQueue.Enqueue(new Token() { value = token, isVariable = true });
                            }
                            //}
                        }
                        else if (pstr[ptr] == '[')
                        {
                            opStack.Push(new OpToken() { value = "[" });
                            ptr++;
                        }
                        else if (pstr[ptr] == ']')
                        {
                            bool pe = false;
                            // Until the token at the top of the stack is a left parenthesis,
                            // pop operators off the stack onto the output queue
                            while (opStack.Count > 0)
                            {
                                OpToken sc = opStack.Peek();
                                if (sc.value == "[")
                                {
                                    pe = true;
                                    break;
                                }
                                else
                                {
                                    OpToken popToken = opStack.Pop();
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type });
                                }
                            }

                            // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                                //printf("Error: parentheses mismatched\n");
                                //return false;
                            }
                            // Pop the left parenthesis from the stack, but not onto the output queue.
                            OpToken lopToken = opStack.Pop();
                            tokenQueue.Enqueue(new Token() { value = lopToken.value, isOperator = true, type = lopToken.type });

                            ptr++;
                        }
                        else if (pstr[ptr] == '(')
                        {
                            opStack.Push(new OpToken() { value = "(" });
                            ptr++;
                        }
                        else if (pstr[ptr] == ')')
                        {
                            bool pe = false;
                            // Until the token at the top of the stack is a left parenthesis,
                            // pop operators off the stack onto the output queue
                            while (opStack.Count > 0)
                            {
                                OpToken sc = opStack.Peek();
                                if (sc.value == "(")
                                {
                                    pe = true;
                                    break;
                                }
                                else
                                {
                                    OpToken popToken = opStack.Pop();
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type });
                                }
                            }

                            // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                                //printf("Error: parentheses mismatched\n");
                                //return false;
                            }
                            // Pop the left parenthesis from the stack, but not onto the output queue.
                            opStack.Pop();
                            // If the token at the top of the stack is a function token, pop it onto the output queue.
                            //if (opStack.Count > 0)
                            //{
                            //char sc = opStack.Pop();
                            //if (is_function(sc))
                            //{
                            //    *outpos = sc;
                            //    ++outpos;
                            //    sl--;
                            //}
                            //}
                            ptr++;
                        }
                        else if ((op = is_operator(pstr, ref ptr)) != null)
                        {
                            while (opStack.Count > 0)
                            {
                                OpToken sc = opStack.Peek();

                                if (is_operator(sc.value) &&
                                     ((operators[op].leftassoc &&
                                       (operators[op].precedence <= operators[sc.value].precedence)) ||
                                       (operators[op].precedence < operators[sc.value].precedence))
                                    )
                                {
                                    OpToken popToken = opStack.Pop();
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type });
                                }
                                else
                                {
                                    break;
                                }
                            }
                            opStack.Push(new OpToken() { value = op });
                            ptr++;
                        }
                        else
                        {
                            throw new Exception("Unexpected token '" + pstr[ptr].ToString() + "'");
                        }
                    }
                    else
                    {
                        ptr++;
                    }
                }

                while (opStack.Count > 0)
                {
                    OpToken sc = opStack.Peek();
                    if (sc.value == "(" || sc.value == ")")
                    {
                        throw new Exception("Paren mismatch");
                    }
                    sc = opStack.Pop();
                    tokenQueue.Enqueue(new Token() { value = sc.value, isOperator = true, type = sc.type });
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Parser error at position {0}: {1}", ptr, ex.Message), ex);
            }
        }

#if TYPE_SAFE
        public T Eval()
#else
        public object Eval()
#endif
        {
            if (compiled == null) Compile();
            return compiled();
        }

        public Expression CreateFunc()
        {
            Queue<Token> tempQueue = new Queue<Token>(tokenQueue);

            Stack<Expression> exprStack = new Stack<Expression>();

            while (tempQueue.Count > 0)
            {
                Token t = tempQueue.Dequeue();

                if (t.isIdent)
                {
                    exprStack.Push(Expression.Constant(t.value, t.type));
                }
                else if (t.isVariable)
                {
                    string token = (string)t.value;
                    exprStack.Push(Expression.Property(Expression.Constant(StateBag), (string)token));
                }
                else if (t.isOperator)
                {
                    Expression result = null;
                    Expression re = null;
                    Expression le = null;
                    Operator op = operators[(string)t.value];

                    switch (op.arguments)
                    {
                        case 1:
                            le = exprStack.Pop();
                            if (op.tFunc != null)
                            {
                                result = op.tFunc(le, t.type);
                            }
                            else
                            {
                                result = op.uFunc(le);
                            }
                            break;
                        case 2:
                            re = exprStack.Pop();
                            le = exprStack.Pop();
                            result = op.bFunc(le, re);
                            break;
                    }

                    exprStack.Push(result);
                }

            }

            if (exprStack.Count == 1)
            {
                return exprStack.Pop();
            }
            else
            {
                throw new Exception("Invalid expression");
            }

            return null;
        }

        private
#if TYPE_SAFE
            Func<T> 
#else
 compiledFunc
#endif
            Compile(Expression exp)
        {
            if (tokenQueue.Count == 0) Parse();
#if TYPE_SAFE
            Expression<Func<T>> f = Expression.Lambda<Func<T>>(exp);
#else
            Expression<compiledFunc> f = Expression.Lambda<compiledFunc>(Expression.Convert(exp, typeof(object)));
#endif
            return f.Compile();
        }

        private bool IsInBounds()
        {
            return ptr < pstr.Length;
        }

        public void Compile()
        {
            compiled = Compile(CreateFunc());
        }

    }
}
