using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ExpressionEvaluator.Operators;
using ExpressionEvaluator.Tokens;
using System.Reflection;

namespace ExpressionEvaluator
{
    public class Parser
    {
        string _pstr;
        int _ptr;

        private readonly Queue<Token> _tokenQueue = new Queue<Token>();
        private readonly Stack<OpToken> _opStack = new Stack<OpToken>();
        private OperatorCollection _operators;

        public TypeRegistry TypeRegistry { get; set; }

        public object Global { get; set; }

        public string StringToParse
        {
            get
            {
                return _pstr;
            }
            set
            {
                _pstr = value;
                _tokenQueue.Clear();
                _ptr = 0;
            }
        }

        public Parser()
        {
            Initialize();
            TypeRegistry = new TypeRegistry();
        }

        public Parser(string str)
        {
            Initialize();
            _pstr = str;
        }

        void Initialize()
        {
            _operators = new OperatorCollection
                {
                    {".", new MethodOperator(".", 12, true, OperatorCustomExpressions.MemberAccess)},
                    {"!", new UnaryOperator("!", 11, false, Expression.Not)},
                    {"*", new BinaryOperator("*", 10, true, Expression.Multiply)},
                    {"/", new BinaryOperator("/", 10, true, Expression.Divide)},
                    {"%", new BinaryOperator("%", 10, true, Expression.Modulo)},
                    {"+", new BinaryOperator("+", 9, true, OperatorCustomExpressions.Add)},
                    {"-", new BinaryOperator("-", 9, true, Expression.Subtract)},
                    {"<<", new BinaryOperator("<<", 8, true, Expression.LeftShift)},
                    {">>", new BinaryOperator(">>", 8, true, Expression.RightShift)},
                    {"<", new BinaryOperator("<", 7, true, Expression.LessThan)},
                    {">", new BinaryOperator(">", 7, true, Expression.GreaterThan)},
                    {"<=", new BinaryOperator("<=", 7, true, Expression.LessThanOrEqual)},
                    {">=", new BinaryOperator(">=", 7, true, Expression.GreaterThanOrEqual)},
                    {"==", new BinaryOperator("==", 6, true, Expression.Equal)},
                    {"!=", new BinaryOperator("!=", 6, true, Expression.NotEqual)},
                    {"&", new BinaryOperator("&", 5, true, Expression.And)},
                    {"^", new BinaryOperator("^", 4, true, Expression.ExclusiveOr)},
                    {"|", new BinaryOperator("|", 3, true, Expression.Or)},
                    {"&&", new BinaryOperator("&&", 2, true, Expression.AndAlso)},
                    {"||", new BinaryOperator("||", 1, true, Expression.OrElse)},
                    {":", new TernarySeparatorOperator(":", 2, false, OperatorCustomExpressions.TernarySeparator)},
                    {"=", new BinaryOperator("=", 1, false, Expression.Assign)},
                    {"?", new TernaryOperator("?", 1, false, Expression.Condition)}
                };

            //operators.Add("^", new BinaryOperator("^", 11, false, Expression.Power));

            //operators.Add("[", new IndexOperator("[", 0, true, OperatorCustomExpressions.ArrayAccess));

        }

        /// <summary>
        /// Returns a boolean specifying if the current string pointer is within the bounds of the expression string
        /// </summary>
        /// <returns></returns>
        private bool IsInBounds()
        {
            return _ptr < _pstr.Length;
        }

        public Expression Parse(string expression)
        {
            StringToParse = expression;
            Parse();
            return BuildTree();
        }

        /// <summary>
        /// Parses the expression and builds the token queue for compiling
        /// </summary>
        public void Parse(bool isScope = false)
        {
            try
            {
                _tokenQueue.Clear();
                _ptr = 0;

                while (IsInBounds())
                {
                    string op = "";

                    int lastptr = _ptr;

                    if (_pstr[_ptr] != ' ')
                    {
                        // Parse enclosed strings
                        if (_pstr[_ptr] == '\'')
                        {
                            bool isStringClosed = false;
                            _ptr++;
                            lastptr = _ptr;
                            StringBuilder tokenbuilder = new StringBuilder();

                            // check for escaped single-quote and backslash
                            while (IsInBounds())
                            {
                                if (_pstr[_ptr] == '\\')
                                {
                                    tokenbuilder.Append(_pstr.Substring(lastptr, _ptr - lastptr));
                                    char nextchar = _pstr[_ptr + 1];
                                    switch (nextchar)
                                    {
                                        case '\'':
                                        case '\\':
                                            tokenbuilder.Append(nextchar);
                                            break;
                                        default:
                                            throw new Exception("Unrecognized escape sequence");
                                    }
                                    _ptr++;
                                    _ptr++;
                                    lastptr = _ptr;
                                }
                                else if ((_pstr[_ptr] == '\''))
                                {
                                    isStringClosed = true;
                                    break;
                                }
                                else
                                {
                                    _ptr++;
                                }
                            }

                            if (!isStringClosed) throw new Exception("Unclosed string literal at " + lastptr);

                            tokenbuilder.Append(_pstr.Substring(lastptr, _ptr - lastptr));
                            string token = tokenbuilder.ToString();
                            _tokenQueue.Enqueue(new Token() { Value = token, IsIdent = true, Type = typeof(string) });
                            _ptr++;
                        }
                        // Parse enclosed dates
                        else if (_pstr[_ptr] == '#')
                        {
                            bool isDateClosed = false;

                            _ptr++;
                            lastptr = _ptr;

                            while (IsInBounds())
                            {
                                _ptr++;
                                if (_pstr[_ptr] == '#')
                                {
                                    isDateClosed = true;
                                    break;
                                }
                            }

                            if (!isDateClosed) throw new Exception("Unclosed date literal at " + lastptr);

                            string token = _pstr.Substring(lastptr, _ptr - lastptr);

                            DateTime dt = DateTime.Parse(token);
                            _tokenQueue.Enqueue(new Token() { Value = dt, IsIdent = true, Type = typeof(DateTime) });
                            _ptr++;

                        }
                        else if (_pstr[_ptr] == ',')
                        {
                            bool pe = false;


                            while (_opStack.Count > 0)
                            {
                                if ((string)_opStack.Peek().Value == "(")
                                {
                                    OpToken temp = _opStack.Pop();
                                    Token lastToken = _opStack.Peek();
                                    if (lastToken.GetType() == typeof(MemberToken))
                                    {
                                        MemberToken lastmember = (MemberToken)lastToken;
                                        if (lastmember != null) lastmember.ArgCount++;
                                    }
                                    _opStack.Push(temp);
                                    pe = true;
                                    break;
                                }
                                else
                                {
                                    OpToken popToken = _opStack.Pop();
                                    _tokenQueue.Enqueue(popToken);
                                }

                            }


                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                            }

                            _ptr++;
                        }
                        // Member accessor
                        else if (_pstr[_ptr] == '.')
                        {
                            if (_opStack.Count > 0)
                            {
                                OpToken sc = _opStack.Peek();
                                // if the last operator was also a Member accessor pop it on the tokenQueue
                                if ((string)sc.Value == ".")
                                {
                                    OpToken popToken = _opStack.Pop();
                                    _tokenQueue.Enqueue(popToken);
                                }
                            }

                            _opStack.Push(new MemberToken());
                            _ptr++;
                        }
                        // Parse hexadecimal literals
                        else if (HelperMethods.IsHexStart(_pstr, _ptr))
                        {
                            bool isNeg = false;
                            if (_pstr[_ptr] == '-')
                            {
                                isNeg = true;
                                _ptr++;
                                lastptr = _ptr;
                            }
                            //skip 0x
                            _ptr += 2;
                            // Number identifiers start with a number and may contain numbers and decimals
                            while (IsInBounds() && (HelperMethods.IsHex(_pstr, _ptr) || _pstr[_ptr] == 'L'))
                            {
                                _ptr++;
                            }

                            string token = _pstr.Substring(lastptr, _ptr - lastptr);

                            Type ntype = typeof(System.Int32);
                            object val = null;

                            if (token.EndsWith("L"))
                            {
                                ntype = typeof(System.Int64);
                                token = token.Remove(token.Length - 1, 1);
                            }

                            switch (ntype.Name)
                            {
                                case "Int32":
                                    val = isNeg ? -Convert.ToInt32(token, 16) : Convert.ToInt32(token, 16);
                                    break;
                                case "Int64":
                                    val = isNeg ? -Convert.ToInt64(token, 16) : Convert.ToInt64(token, 16);
                                    break;
                            }

                            _tokenQueue.Enqueue(new Token() { Value = val, IsIdent = true, Type = ntype });
                        }
                        // Parse numbers
                        else if (HelperMethods.IsANumber(_pstr, _ptr))
                        {
                            bool isDecimal = false;
                            int suffixStart = 0;
                            int suffixLength = 0;
                            // Number identifiers start with a number and may contain numbers and decimals
                            bool exit = false;

                            while (IsInBounds() && !exit)
                            {
                                if (_pstr[_ptr] == 'l' || _pstr[_ptr] == 'L' || _pstr[_ptr] == 'u' || _pstr[_ptr] == 'U')
                                {
                                    if (suffixLength == 0) suffixStart = _ptr;
                                    if (suffixLength == 1) exit = true;
                                    suffixLength++;
                                }
                                else if (_pstr[_ptr] == '.')
                                {
                                    if (isDecimal) break;
                                    isDecimal = true;
                                }
                                else if (_pstr[_ptr] == 'd' || _pstr[_ptr] == 'D' || _pstr[_ptr] == 'f' || _pstr[_ptr] == 'F' || _pstr[_ptr] == 'm' || _pstr[_ptr] == 'M')
                                {
                                    suffixStart = _ptr;
                                    suffixLength++;
                                    exit = true;
                                }
                                else if (_pstr[_ptr] == 'l' || _pstr[_ptr] == 'L' || _pstr[_ptr] == 'u' || _pstr[_ptr] == 'U')
                                {
                                    if (isDecimal) throw new Exception("Expected end of decimal literal");
                                    suffixStart = _ptr;
                                    exit = true;
                                }
                                else if (!HelperMethods.IsANumber(_pstr, _ptr))
                                {
                                    break;
                                }
                                _ptr++;
                            }

                            string token = _pstr.Substring(lastptr, _ptr - lastptr);
                            string suffix = "";

                            Type ntype = null;
                            object val = null;

                            if (suffixLength > 0)
                            {
                                suffix = token.Substring(token.Length - suffixLength);
                                token = token.Substring(0, token.Length - suffixLength);

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
                                if (isDecimal)
                                {
                                    ntype = typeof(Double);
                                    val = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    ntype = typeof(Int32);
                                    val = int.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }


                            _tokenQueue.Enqueue(new Token() { Value = val, IsIdent = true, Type = ntype });
                        }
                        // Test for identifier
                        else if (HelperMethods.IsAlpha(_pstr[_ptr]) || (_pstr[_ptr] == '_'))
                        {
                            _ptr++;

                            while (IsInBounds() && (HelperMethods.IsAlpha(_pstr[_ptr]) || (_pstr[_ptr] == '_') || HelperMethods.IsNumeric(_pstr, _ptr)))
                            {
                                _ptr++;
                            }


                            string token = _pstr.Substring(lastptr, _ptr - lastptr);
                            MemberToken mToken = null;

                            if (_opStack.Count > 0)
                            {
                                OpToken opToken = _opStack.Peek();
                                if (opToken.GetType() == typeof(MemberToken))
                                    mToken = (MemberToken)opToken;
                            }

                            if ((mToken != null) && (mToken.Name == null))
                            {
                                mToken.Name = token;
                            }
                            else if (TypeRegistry.ContainsKey(token))
                            {
                                if (TypeRegistry[token].GetType().Name == "RuntimeType")
                                {
                                    _tokenQueue.Enqueue(new Token() { Value = ((Type)TypeRegistry[token]).UnderlyingSystemType, IsType = true });
                                }
                                else
                                {
                                    _tokenQueue.Enqueue(new Token() { Value = TypeRegistry[token], IsType = true });
                                }
                            }
                            else
                            {
                                if ((token.ToLower() == "null"))
                                {
                                    _tokenQueue.Enqueue(new Token() { Value = null, IsIdent = true, Type = typeof(object) });
                                }
                                else if ((token.ToLower() == "true") || (token.ToLower() == "false"))
                                {
                                    _tokenQueue.Enqueue(new Token() { Value = Boolean.Parse(token), IsIdent = true, Type = typeof(Boolean) });
                                }
                                else
                                {
                                    if (Global != null)
                                    {
                                        _tokenQueue.Enqueue(new Token() { Value = Global, IsType = true });
                                    }
                                    else
                                    {
                                        if (isScope)
                                        {
                                            _tokenQueue.Enqueue(new Token() { IsScope = true });
                                        }
                                        else
                                        {
                                            throw new Exception(string.Format("Unknown type or identifier '{0}'", token));
                                        }
                                    }

                                    if (_opStack.Count > 0)
                                    {
                                        OpToken sc = _opStack.Peek();
                                        // if the last operator was also a Member accessor pop it on the tokenQueue
                                        if ((string)sc.Value == ".")
                                        {
                                            OpToken popToken = _opStack.Pop();
                                            _tokenQueue.Enqueue(popToken);
                                        }
                                    }

                                    _opStack.Push(new MemberToken());
                                    _ptr -= token.Length;
                                }
                            }
                        }
                        else if (_pstr[_ptr] == '[')
                        {
                            _opStack.Push(new OpToken() { Value = "[", Ptr = _ptr + 1 });
                            _ptr++;
                        }
                        else if (_pstr[_ptr] == ']')
                        {
                            bool pe = false;
                            // Until the token at the top of the stack is a left bracket,
                            // pop operators off the stack onto the output queue
                            while (_opStack.Count > 0)
                            {
                                OpToken sc = _opStack.Peek();
                                if ((string)sc.Value == "[")
                                {
                                    OpToken temp = _opStack.Pop();
                                    if (_opStack.Count > 0)
                                    {
                                        Token lastToken = _opStack.Peek();
                                        if (lastToken.GetType() == typeof(MemberToken))
                                        {
                                            MemberToken lastmember = (MemberToken)lastToken;
                                            // check if there was anything significant between the opening paren and the closing paren
                                            // If so, then we have an argument... This isn't the best approach perhaps, but it works...
                                            if (_pstr.Substring(sc.Ptr, _ptr - sc.Ptr).Trim().Length > 0) lastmember.ArgCount++;
                                        }
                                    }
                                    _opStack.Push(temp);
                                    pe = true;
                                    break;
                                }
                                else
                                {
                                    OpToken popToken = _opStack.Pop();
                                    _tokenQueue.Enqueue(popToken);
                                }
                            }

                            // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                            }

                            // Pop the left parenthesis from the stack, but not onto the output queue.
                            OpToken lopToken = _opStack.Pop();
                            //tokenQueue.Enqueue(lopToken);


                            _ptr++;
                        }
                        else if (_pstr[_ptr] == '(')
                        {
                            int curptr = _ptr;
                            while (_pstr[curptr] != ')')
                            {
                                curptr++;
                            }
                            string typeName = _pstr.Substring(lastptr + 1, curptr - lastptr - 1).Trim();
                            Type t;
                            if (TypeRegistry.ContainsKey(typeName))
                            {
                                _tokenQueue.Enqueue(new Token() { Value = "(" + typeName + ")", IsCast = true, Type = (Type)TypeRegistry[typeName] });
                                _ptr = curptr + 1;
                            }
                            else if ((t = Type.GetType(typeName)) != null)
                            {
                                _tokenQueue.Enqueue(new Token() { Value = "(" + t.Name + ")", IsCast = true, Type = t });
                                _ptr = curptr + 1;
                            }
                            else
                            {
                                _opStack.Push(new OpToken() { Value = "(", Ptr = _ptr + 1 });
                                _ptr++;
                            }

                        }
                        else if (_pstr[_ptr] == ')')
                        {
                            bool pe = false;
                            //int poppedtokens = 0;
                            // Until the token at the top of the stack is a left parenthesis,
                            // pop operators off the stack onto the output queue
                            while (_opStack.Count > 0)
                            {
                                OpToken sc = _opStack.Peek();
                                if ((string)sc.Value == "(")
                                {
                                    OpToken temp = _opStack.Pop();
                                    if (_opStack.Count > 0)
                                    {
                                        Token lastToken = _opStack.Peek();
                                        if (lastToken.GetType() == typeof(MemberToken))
                                        {
                                            MemberToken lastmember = (MemberToken)lastToken;
                                            // check if there was anything significant between the opening paren and the closing paren
                                            // If so, then we have an argument... This isn't the best approach perhaps, but it works...
                                            if (_pstr.Substring(sc.Ptr, _ptr - sc.Ptr).Trim().Length > 0) lastmember.ArgCount++;
                                        }
                                    }
                                    _opStack.Push(temp);
                                    pe = true;
                                    break;
                                }
                                else
                                {
                                    OpToken popToken = _opStack.Pop();
                                    _tokenQueue.Enqueue(popToken);
                                    // poppedtokens++;
                                }
                            }

                            // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                            if (!pe)
                            {
                                throw new Exception("Parenthesis mismatch");
                            }

                            // Pop the left parenthesis from the stack, but not onto the output queue.
                            _opStack.Pop();

                            //If the token at the top of the stack is a function token, pop it onto the output queue.
                            if (_opStack.Count > 0)
                            {
                                OpToken popToken = _opStack.Peek();
                                if ((string)popToken.Value == ".")
                                {
                                    popToken = _opStack.Pop();
                                    _tokenQueue.Enqueue(popToken);
                                }
                            }
                            _ptr++;
                        }
                        else if ((op = _operators.IsOperator(_pstr, ref _ptr)) != null)
                        {
                            while (_opStack.Count > 0)
                            {
                                OpToken sc = _opStack.Peek();

                                if (_operators.IsOperator((string)sc.Value) &&
                                     ((_operators[op].LeftAssoc &&
                                       (_operators[op].Precedence <= _operators[(string)sc.Value].Precedence)) ||
                                       (_operators[op].Precedence < _operators[(string)sc.Value].Precedence))
                                    )
                                {
                                    OpToken popToken = _opStack.Pop();
                                    _tokenQueue.Enqueue(popToken);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            _opStack.Push(new OpToken() { Value = op });
                            _ptr++;
                        }
                        else
                        {
                            throw new Exception("Unexpected token '" + _pstr[_ptr].ToString() + "'");
                        }
                    }
                    else
                    {
                        _ptr++;
                    }
                }

                while (_opStack.Count > 0)
                {
                    OpToken sc = _opStack.Peek();
                    if ((string)sc.Value == "(" || (string)sc.Value == ")")
                    {
                        throw new Exception("Paren mismatch");
                    }

                    sc = _opStack.Pop();
                    _tokenQueue.Enqueue(sc);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Parser error at position {0}: {1}", _ptr, ex.Message), ex);
            }
        }

        /// <summary>
        /// Builds the expression tree from the token queue
        /// </summary>
        /// <returns></returns>
        public Expression BuildTree(Expression scopeParam = null)
        {
            if (_tokenQueue.Count == 0) Parse(scopeParam != null);

            // make a copy of the queue, so that we don't empty the original queue
            Queue<Token> tempQueue = new Queue<Token>(_tokenQueue);
            Stack<Expression> exprStack = new Stack<Expression>();
            List<Expression> args = new List<Expression>();
            Stack<String> literalStack = new Stack<String>();

#if DEBUG
            var q = tempQueue.Select(x => (x.Value ?? "<null>").ToString() + (x.GetType() == typeof(MemberToken) ? ":" + ((MemberToken)x).Name : ""));
            System.Diagnostics.Debug.WriteLine(string.Join("][", q.ToArray()));
#endif
            int isCastPending = -1;
            Type typeCast = null;

            while (tempQueue.Count > 0)
            {
                Token t = tempQueue.Dequeue();

                if (isCastPending > -1) isCastPending--;
                if (isCastPending == 0)
                {
                    exprStack.Push(Expression.Convert(exprStack.Pop(), typeCast));
                    isCastPending = -1;
                }

                if (t.IsIdent)
                {
                    // handle numeric literals
                    exprStack.Push(Expression.Constant(t.Value, t.Type));
                }
                else if (t.IsType)
                {
                    exprStack.Push(Expression.Constant(t.Value));
                }
                else if (t.IsScope)
                {
                    if (scopeParam == null)
                    {
                        throw new Exception(string.Format("Unexpected identifier {0} or scope empty", t.Value));
                    }
                    exprStack.Push(scopeParam);
                }
                else if (t.IsOperator)
                {
                    // handle operators
                    Expression result = null;
                    var op = _operators[(string)t.Value];
                    var opfunc = OpFuncServiceLocator.Resolve(op.GetType());
                    for (int i = 0; i < t.ArgCount; i++)
                    {
                        args.Add(exprStack.Pop());
                    }
                    // Arguments are in reverse order
                    args.Reverse();
                    result = opfunc(new OpFuncArgs() { TempQueue = tempQueue, ExprStack = exprStack, T = t, Op = op, Args = args, ScopeParam = scopeParam });
                    args.Clear();
                    exprStack.Push(result);
                }
                else if (t.IsCast)
                {
                    isCastPending = 2;
                    typeCast = t.Type;
                }
            }

            // we should only have one complete expression on the stack, otherwise, something went wrong
            if (exprStack.Count == 1)
            {
                Expression pop = exprStack.Pop();
#if DEBUG
                System.Diagnostics.Debug.WriteLine(pop.ToString());
#endif
                return pop;
            }
            else
            {
                throw new Exception("Invalid expression");
            }

            return null;
        }

    }
}
