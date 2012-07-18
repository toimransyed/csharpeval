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
        public bool isParameterizer;
        public bool isProperty;
        public bool isFunction;
        public int argCount;
    }


    internal class OpToken
    {
        public string value;
        public Type type;
        public int ptr;
    }

    public class Parser
    {
        string pstr;
        int ptr = 0;
        public object StateBag { get; set; }
        Queue<Token> tokenQueue = new Queue<Token>();
        Stack<OpToken> opStack = new Stack<OpToken>();
        Stack<string> funcStack = new Stack<string>();

        static Dictionary<string, object> typeRegistry = new Dictionary<string, object>();

        public static void RegisterDefaultTypes()
        {
            if (typeRegistry.Count == 0)
            {
                typeRegistry.Add("bool", typeof(System.Boolean));
                typeRegistry.Add("byte", typeof(System.Byte));
                typeRegistry.Add("char", typeof(System.Char));
                typeRegistry.Add("int", typeof(System.Int32));
                typeRegistry.Add("decimal", typeof(System.Decimal));
                typeRegistry.Add("double", typeof(System.Double));
                typeRegistry.Add("float", typeof(System.Single));
                typeRegistry.Add("object", typeof(System.Object));
                typeRegistry.Add("string", typeof(System.String));
                typeRegistry.Add("DateTime", typeof(System.DateTime));
                typeRegistry.Add("Convert", typeof(System.Convert));
                typeRegistry.Add("Math", typeof(System.Math));
            }
        }

        public void RegisterType(string key, object type)
        {
            typeRegistry.Add(key, type);
        }

        static OperatorCollection operators;
        Func<object> compiled = null;

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

        static Dictionary<Type, int> typePrecedence;

        internal static void ImplicitConversion(ref Expression le, ref Expression re)
        {
            if (typePrecedence.ContainsKey(le.Type) && typePrecedence.ContainsKey(re.Type))
            {
                if (typePrecedence[le.Type] > typePrecedence[re.Type]) re = Expression.Convert(re, le.Type);
                if (typePrecedence[le.Type] < typePrecedence[re.Type]) le = Expression.Convert(le, re.Type);
            }
        }

        static Expression ImplicitConversion(Expression le, Type type)
        {
            if (typePrecedence.ContainsKey(le.Type) && typePrecedence.ContainsKey(type))
            {
                if (typePrecedence[le.Type] < typePrecedence[type]) return Expression.Convert(le, type);
            }
            return le;
        }
        static OpFuncServiceLocator opfuncs;

        static void Initialize()
        {
            opfuncs = new OpFuncServiceLocator();
            operators = new OperatorCollection();

            operators.Add(".", new MethodOperator(".", 7, true,
                delegate(Expression le, string token, List<Expression> args)
                {
                    List<Type> argTypes = new List<Type>();
                    args.ForEach(x => argTypes.Add(x.Type));

                    Expression instance = null;
                    Type type = null;
                    if (le.Type.Name == "RuntimeType")
                    {
                        type = ((Type)((ConstantExpression)le).Value);
                    }
                    else
                    {
                        type = le.Type;
                        instance = le;
                    }

                    MethodInfo mi = type.GetMethod(token, argTypes.ToArray());
                    if (mi != null)
                    {
                        ParameterInfo[] pi = mi.GetParameters();
                        for (int i = 0; i < pi.Length; i++)
                        {
                            args[i] = ImplicitConversion(args[i], pi[i].ParameterType);
                        }
                        return Expression.Call(instance, mi, args);
                    }
                    else
                    {
                        PropertyInfo pi = type.GetProperty(token);
                        if (pi != null)
                        {
                            return Expression.Property(instance, pi);
                        }
                        else
                        {
                            FieldInfo fi = type.GetField(token);
                            if (fi != null)
                                return Expression.Field(instance, fi);
                        }
                        throw new Exception(string.Format("Member not found: {0}.{1}", le.Type.Name, token));
                    }
                }
                ));

            operators.Add("!", new UnaryOperator("!", 6, false, Expression.Not));
            operators.Add("^", new BinaryOperator("^", 6, false, Expression.Power));
            operators.Add("*", new BinaryOperator("*", 5, true, Expression.Multiply));
            operators.Add("/", new BinaryOperator("/", 5, true, Expression.Divide));
            operators.Add("%", new BinaryOperator("%", 5, true, Expression.Modulo));
            operators.Add("+", new BinaryOperator("+", 4, true,
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
            operators.Add("-", new BinaryOperator("-", 4, true, Expression.Subtract));
            operators.Add("==", new BinaryOperator("==", 3, true, Expression.Equal));
            operators.Add("!=", new BinaryOperator("!=", 3, true, Expression.NotEqual));
            operators.Add("<", new BinaryOperator("<", 3, true, Expression.LessThan));
            operators.Add(">", new BinaryOperator(">", 3, true, Expression.GreaterThan));
            operators.Add("<=", new BinaryOperator("<=", 3, true, Expression.LessThanOrEqual));
            operators.Add(">=", new BinaryOperator(">=", 3, true, Expression.GreaterThanOrEqual));
            operators.Add("&&", new BinaryOperator("&&", 2, true, Expression.And));
            operators.Add("||", new BinaryOperator("||", 1, true, Expression.Or));

            operators.Add("[", new BinaryOperator("[", 0, true,
                delegate(Expression le, Expression re)
                {
                    return Expression.ArrayAccess(le, re);
                }
                ));

            // for implicit conversion
            typePrecedence = new Dictionary<Type, int>();
            typePrecedence.Add(typeof(byte), 0);
            typePrecedence.Add(typeof(int), 1);
            typePrecedence.Add(typeof(float), 2);
            typePrecedence.Add(typeof(double), 3);
        }

        /// <summary>
        /// Returns a boolean specifying if the current string pointer is within the bounds of the expression string
        /// </summary>
        /// <returns></returns>
        private bool IsInBounds()
        {
            return ptr < pstr.Length;
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

        /// <summary>
        /// Returns a boolean specifying if the character at the current pointer is a valid number
        /// i.e. it starts with a number or a minus sign immediately followed by a number
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        static bool isaNumber(string str, int ptr)
        {
            return
                (!isNumeric(str, ptr - 1) & (str[ptr] == '-') & isNumeric(str, ptr + 1)) ||
                isNumeric(str, ptr);
        }

        /// <summary>
        /// Returns a boolean specifying if the character at the current point is of the range '0'..'9'
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        static bool isNumeric(string str, int ptr)
        {
            if ((ptr >= 0) & (ptr < str.Length))
                return (str[ptr] >= '0' & str[ptr] <= '9');
            else
                return true;
        }


        static bool isAlpha(char chr)
        {
            return (chr >= 'A' & chr <= 'Z') || (chr >= 'a' & chr <= 'z');
        }

        /// <summary>
        /// Parses the expression and builds the token queue for compiling
        /// </summary>
        public void Parse()
        {
            try
            {
                tokenQueue.Clear();
                Stack<int> argCountStack = new Stack<int>();
                int argCount = 0;
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
                            lastptr = ptr;
                            StringBuilder tokenbuilder = new StringBuilder();

                            // check for escaped single-quote and backslash
                            while (IsInBounds())
                            {
                                if (pstr[ptr] == '\\')
                                {
                                    tokenbuilder.Append(pstr.Substring(lastptr, ptr - lastptr));
                                    char nextchar = pstr[ptr + 1];
                                    switch (nextchar)
                                    {
                                        case '\'':
                                        case '\\':
                                            tokenbuilder.Append(nextchar);
                                            break;
                                        default:
                                            throw new Exception("Unrecognized escape sequence");
                                    }
                                    ptr++;
                                    ptr++;
                                    lastptr = ptr;
                                }
                                else if ((pstr[ptr] == '\''))
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
                        else if (pstr[ptr] == ',')
                        {
                            bool pe = false;

                            while (opStack.Count > 0)
                            {
                                if (opStack.Peek().value == "(")
                                {
                                    pe = true;
                                    break;
                                }
                                else
                                {
                                    OpToken popToken = opStack.Pop();
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type, argCount = argCount });
                                    if (argCountStack.Count > 0)
                                        argCount = argCountStack.Pop();
                                }

                            }


                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                            }

                            argCount++;
                            //tokenQueue.Enqueue(new Token() { value = ",", isParameterizer = true });

                            ptr++;
                        }
                        // Member accessor
                        else if (pstr[ptr] == '.')
                        {
                            if (opStack.Count > 0)
                            {
                                OpToken sc = opStack.Peek();
                                // if the last operator was also a Member accessor pop it on the tokenQueue
                                if (sc.value == ".")
                                {
                                    OpToken popToken = opStack.Pop();
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type, argCount = argCount });
                                    if (argCountStack.Count > 0)
                                        argCount = argCountStack.Pop();
                                }
                            }

                            // Save the current argument count as we are starting a new function/property
                            argCountStack.Push(argCount);
                            argCount = 0;

                            opStack.Push(new OpToken() { value = "." });
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
                            // Alphanumeric identifiers start with a letter and may contain letter, numbers 
                            while (IsInBounds() && (isAlpha(pstr[ptr]) || isNumeric(pstr, ptr)))
                            {
                                ptr++;
                            }

                            string token = pstr.Substring(lastptr, ptr - lastptr);


                            if (typeRegistry.ContainsKey(token))
                            {
                                if (typeRegistry[token].GetType().Name == "RuntimeType")
                                {
                                    tokenQueue.Enqueue(new Token() { value = ((Type)typeRegistry[token]).UnderlyingSystemType, isType = true });
                                }
                                else
                                {
                                    tokenQueue.Enqueue(new Token() { value = typeRegistry[token], isType = true });
                                }
                            }
                            else
                            {
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
                                    if (opStack.Count > 0 && opStack.Peek().value == ".")
                                        tokenQueue.Enqueue(new Token() { value = token });
                                    else
                                        throw new Exception(string.Format("Unknown type or identifier '{0}'", token));
                                }
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
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type, argCount = argCount });
                                    if (argCountStack.Count > 0)
                                        argCount = argCountStack.Pop();
                                }
                            }

                            // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
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
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type, argCount = argCount });
                                    if (argCountStack.Count > 0)
                                        argCount = argCountStack.Pop();
                                }
                            }

                            // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                            }

                            // Pop the left parenthesis from the stack, but not onto the output queue.
                            opStack.Pop();

                            //If the token at the top of the stack is a function token, pop it onto the output queue.
                            if (opStack.Count > 0)
                            {
                                OpToken popToken = opStack.Peek();
                                if (popToken.value == ".")
                                {
                                    popToken = opStack.Pop();
                                    tokenQueue.Enqueue(new Token() { value = popToken.value, isOperator = true, type = popToken.type, argCount = argCount });
                                    if (argCountStack.Count > 0)
                                        argCount = argCountStack.Pop();
                                }
                            }
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
                                    argCount = argCountStack.Pop();
                                }
                                else
                                {
                                    break;
                                }
                            }

                            argCountStack.Push(argCount);
                            argCount = 0;
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

        /// <summary>
        /// Executes the cached compiled expression
        /// </summary>
        /// <returns></returns>
        public object Eval()
        {
            if (compiled == null) Compile();
            return compiled();
        }

        /// <summary>
        /// Builds the expression tree from the token queue
        /// </summary>
        /// <returns></returns>
        public Expression BuildTree()
        {
            // make a copy of the queue, so that we don't empty the original queue
            Queue<Token> tempQueue = new Queue<Token>(tokenQueue);
            Stack<Expression> exprStack = new Stack<Expression>();
            List<Expression> args = new List<Expression>();
            Stack<String> literalStack = new Stack<String>();

            var q = tempQueue.Select(x => x.value.ToString() + (x.isOperator ? ":" + x.argCount.ToString() : "") );
            System.Diagnostics.Debug.WriteLine(string.Join("][", q.ToArray()));


            while (tempQueue.Count > 0)
            {
                Token t = tempQueue.Dequeue();

                if (t.isIdent)
                {
                    // handle numeric literals
                    exprStack.Push(Expression.Constant(t.value, t.type));
                }
                else if (t.isType)
                {
                    exprStack.Push(Expression.Constant(t.value));
                }
                else if (t.isVariable)
                {
                    // handle variables
                    string token = (string)t.value;
                    exprStack.Push(Expression.Property(Expression.Constant(StateBag), (string)token));
                }
                else if (t.isParameterizer)
                {
                    //exprStack.Push(Expression.Property(Expression.Constant(StateBag), (string)token));
                }
                else if (t.isOperator)
                {
                    // handle operators
                    Expression result = null;
                    IOperator op = operators[(string)t.value];
                    OpFuncDelegate opfunc = opfuncs.Resolve(op.GetType());
                    for (int i = 0; i < t.argCount; i++)
                    {
                        args.Add(exprStack.Pop());
                    }
                    // Arguments are in reverse order
                    args.Reverse();
                    result = opfunc(new OpFuncArgs() { tempQueue = tempQueue, exprStack = exprStack, t = t, op = op, args = args, literalStack = literalStack });
                    args.Clear();
                    exprStack.Push(result);
                }
                else
                {
                    //tempQueue.Enqueue(t);
                    literalStack.Push((string)t.value);
                }

            }

            // we should only have one complete expression on the stack, otherwise, something went wrong
            if (exprStack.Count == 1)
            {
                Expression pop = exprStack.Pop();
                System.Diagnostics.Debug.WriteLine(pop.ToString());
                return pop;
            }
            else
            {

                throw new Exception("Invalid expression");
            }

            return null;
        }

        /// <summary>
        /// Returns a type-safe delegate that represents the compiled expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Func<T> Compile<T>()
        {
            return Expression.Lambda<Func<T>>(BuildTree()).Compile();
        }

        /// <summary>
        /// Compiles the expression with an implicit conversion to the object Type and sets the internal function cache to the compiled expression
        /// </summary>
        /// <returns></returns>
        public Func<object> Compile()
        {
            compiled = Expression.Lambda<Func<object>>(Expression.Convert(BuildTree(), typeof(object))).Compile();
            return compiled;
        }

    }
}
