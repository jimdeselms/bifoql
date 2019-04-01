namespace Bifoql
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Bifoql.Expressions;
    using Bifoql.Adapters;
    using Bifoql.Lex;
    using Bifoql.Expressions.Builtins;

    internal class QueryParser
    {
        private readonly IReadOnlyDictionary<string, CustomFunction> _customFunctions;
        private readonly Expr _immediateResult = null;
        private readonly IReadOnlyList<Token> _tokens;
        private int _i;

        internal static Expr Parse(string query, IReadOnlyDictionary<string, CustomFunction> customFunctions)
        {
            var parser = new QueryParser(query, customFunctions);
            if (parser._immediateResult != null)
            {
                return parser._immediateResult;
            }
            else
            {
                return parser.Parse();
            }
        }

        private QueryParser(string query, IReadOnlyDictionary<string, CustomFunction> customFunctions)
        {
            _customFunctions = customFunctions ?? new Dictionary<string, CustomFunction>();
            _tokens = ParseTokens(query, out _immediateResult);
            _i = 0;
        }

        private IReadOnlyList<Token> ParseTokens(string query, out Expr immediateResult)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    immediateResult = new IdentityExpr(new Location(1, 1));
                    return null;
                }

                var lexer = new Lexer(
                    operators: new [] { 
                        "|", "|<", "&", ".", "(", "[?", "[", "{", "}", "]", ")", ".", "@", "*", 
                        "?", ":", ",", "-", "+", "/", "%", "==", "!=", "&&", "||", "??", "..",
                        "<", "<=", ">", ">=", "=", "$", ";", "..." },
                    charKind: "STRING",
                    charsMustBeOneChar: false,
                    backtickStringKind: "STRING",
                    intKind: "NUMBER",
                    floatKind: "NUMBER",
                    stringKind: "STRING",
                    hasVariables: true
                );

                // Take a guess as to the size of the list so that it doesn't have to get rebuilt a bunch.
                var tokens = new List<Token>(query.Length / 3);
                foreach (var token in lexer.Parse(query))
                {
                    tokens.Add(token);
                }

                immediateResult = null;
                return tokens;
            }
            catch (ParseException ex)
            {
                immediateResult = new ErrorExpr(ex.Location, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                immediateResult = new ErrorExpr(new Location(0, 0), ex.Message);
                return null;
            }
        }

        internal Expr Parse()
        {
            try
            {
                return ParseExpr();
            }
            catch (ParseException ex)
            {
                return new ErrorExpr(ex.Location, ex.Message);
            }
            catch (Exception ex)
            {
                return new ErrorExpr(new Location(0, 0), ex.Message);
            }
        }

        private Expr ParseExpr()
        {
            return ParseAssignment();
        }

        private Expr ParseAssignment()
        {
            var token = GetToken();
            if (GetToken().Kind == "VARIABLE" && GetToken(1).Kind == "=")
            {
                var variable = Match("VARIABLE");
                Match("=");

                var variableValue = ParsePipe();

                Match(";");

                var pipedInto = ParseExpr();

                return new AssignmentExpr(GetLocation(token), variable.Text, variableValue, pipedInto);
            }
            else
            {
                return ParsePipe();
            }
        }

        private Expr ParsePipe()
        {
            var expr = ParseTernaryExpr();

            var token = GetToken();

            if (token.Kind == "|")
            {
                Match("|");

                var next = ParsePipe();
                return new ChainExpr(expr, next, ChainBehavior.OneToOne);
            }
            else if (token.Kind == "|<")
            {
                Match("|<");

                var next = ParsePipe();
                return new ChainExpr(expr, next, ChainBehavior.ToMultiple);
            }
            else
            {
                return expr;
            }
        }

        private Expr ParseTernaryExpr()
        {
            var expr = ParseNullCoalescingExpr();

            if (GetToken().Kind == "?")
            {
                Match("?");

                var ifTrue = ParseTernaryExpr();
                Match(":");
                var ifFalse = ParseTernaryExpr();

                return new TernaryExpr(expr, ifTrue, ifFalse);
            }
            else
            {
                return expr;
            }
        }

        private Expr ParseNullCoalescingExpr()
        {
            var lhs = ParseOrExpr();

            var token = GetToken();
            if (token.Kind == "??")
            {
                Match(token.Kind);

                var rhs = ParseNullCoalescingExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseOrExpr()
        {
            var lhs = ParseAndExpr();

            var token = GetToken();
            if (token.Kind == "||")
            {
                Match(token.Kind);

                var rhs = ParseOrExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseAndExpr()
        {
            var lhs = ParseEqualityExpr();

            var token = GetToken();
            if (token.Kind == "&&")
            {
                Match(token.Kind);

                var rhs = ParseAndExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseEqualityExpr()
        {
            var lhs = ParseInequalityExpr();

            var token = GetToken();
            if (token.Kind == "==" || token.Kind == "!=")
            {
                Match(token.Kind);

                var rhs = ParseEqualityExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseInequalityExpr()
        {
            var lhs = ParseAdditiveExpr();

            var token = GetToken();
            if (token.Kind == "<" || token.Kind == ">" || token.Kind == "<=" || token.Kind == ">=")
            {
                Match(token.Kind);

                var rhs = ParseInequalityExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseAdditiveExpr()
        {
            var lhs = ParseMultiplicativeExpr();

            var token = GetToken();
            if (token.Kind == "+" || token.Kind == "-")
            {
                Match(token.Kind);

                var rhs = ParseAdditiveExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseMultiplicativeExpr()
        {
            var lhs = ParseUnaryExpr();

            var token = GetToken();
            if (token.Kind == "*" || token.Kind == "/" || token.Kind == "%")
            {
                Match(token.Kind);

                var rhs = ParseMultiplicativeExpr();

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseUnaryExpr()
        {
            var token = GetToken();
            if (token.Kind == "&")
            {
                Match("&");

                var innerExpression = ParseUnaryExpr();
                return new ExpressionExpr(innerExpression);
            }
            else if (token.Kind == "-")
            {
                Match("-");

                var innerExpression = ParseUnaryExpr();
                return new UnaryExpr(GetLocation(token), "-", innerExpression);
            }
            else if (token.Kind == "*")
            {
                Match("*");

                // This is a little gross; the evaluation expression should probaby be a real 
                // expression, but we have the function handy so might as well just use it.
                var innerExpression = ParseUnaryExpr();
                return new TypedFunctionCallExpr<IBifoqlExpression>(
                    GetLocation(token), 
                    "eval", 
                    new List<Expr> {innerExpression}, 
                    BuiltinFunctions.Eval);
            }
            else
            {
                return ParseChain();
            }
        }

        private Expr ParseChain()
        {
            var expr = ParseFunctionCallExpr();

            var peek = GetToken();
            if (peek.Kind == "." || peek.Kind == "[" || peek.Kind == "(" || peek.Kind == "{" || peek.Kind == "[?")
            {
                return ParseChainRemainder(expr);
            }
            else
            {
                return expr;
            }
        }

        private Expr ParseChainRemainder(Expr prev)
        {
            var token = GetToken();
            Expr first = null;
            if (token.Kind == ".")
            {
                Match(".");
                first = ParseKeyExpr(prev);
            }
            else if (token.Kind == "[?")
            {
                first = ParseFilterExpr(prev);
            }
            else if (token.Kind == "[")
            {
                first = ParseIndexExpr(prev);
            }
            else if (token.Kind == "(")
            {
                // Super gross; in retrospect, all expressions should just be self-contained, or this
                // should at least work the same as the other chain expressions.
                first = ParseIndexedLookup(prev);
            }
            else if (token.Kind == "{")
            {
                var map = ParseMapProjectionExpr(null);
                first = new ChainExpr(prev, map, ChainBehavior.ToMultipleIfArray);
            }

            var nextToken = GetToken();
            if (nextToken.Kind == "." || nextToken.Kind == "[" || nextToken.Kind == "(" || nextToken.Kind == "{" || nextToken.Kind == "[?")
            {
                return ParseChainRemainder(first);
            }
            else
            {
                return first;
            }
        }

        private bool NextIsChainAtomicExpr(int i)
        {
            // Does the next token tell us that we're continuing a chain expression?
            var peek = GetToken();
            return (peek.Kind == "." );
        }

        private static readonly HashSet<string> _builtinFunctionNames = new HashSet<string>()
        {
            "abs",
            "avg",
            "ceil",
            "contains",
            "distinct",
            "ends_with",
            "error",
            "flatten",
            "floor",
            "if_error",
            "join",
            "keys",
            "length",
            "max",
            "max_by",
            "min",
            "min_by",
            "reverse",
            "sort",
            "sort_by",
            "starts_with",
            "sum",
            "to_number",
            "to_map",
            "type",
            "unzip",
            "values",
            "zip",
        };

        private Expr ParseFunctionCallExpr()
        {
            var id = GetToken();
            if (id.Kind != "ID" || (!_builtinFunctionNames.Contains(id.Text) && !_customFunctions.ContainsKey(id.Text))) 
            {
                return ParseIndexedLookup();
            }

            var functionNameToken = Match("ID");
            var arguments = ParseArgumentList().ToList();

            var location = GetLocation(functionNameToken);
            var functionName = functionNameToken.Text;

            CustomFunction customFunction;
            if (_customFunctions.TryGetValue(functionNameToken.Text, out customFunction))
            {
                return customFunction.ToExpr(location, functionNameToken.Text, arguments);
            }

            switch (functionNameToken.Text)
            {
                case "abs": return new TypedFunctionCallExpr<IBifoqlNumber>(location, "abs", arguments, BuiltinFunctions.Abs);
                case "avg": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "avg", arguments, BuiltinFunctions.Avg);
                case "ceil": return new TypedFunctionCallExpr<IBifoqlNumber>(location, "ceil", arguments, BuiltinFunctions.Ceil);
                case "contains": return new BinaryExpr(arguments[0], "contains", arguments[1]);
                case "distinct": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "distinct", arguments, BuiltinFunctions.Distinct);
                case "ends_with": return new BinaryExpr(arguments[0], "ends_with", arguments[1]);
                case "error": return new ErrorFunctionExpr(location, arguments);
                case "flatten": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "flatten", arguments, BuiltinFunctions.Flatten);
                case "floor": return new TypedFunctionCallExpr<IBifoqlNumber>(location, "floor", arguments, BuiltinFunctions.Floor);
                case "join": return new TypedFunctionCallExpr<IBifoqlString, IBifoqlArrayInternal>(location, "join", arguments, BuiltinFunctions.Join);
                case "keys": return new TypedFunctionCallExpr<IBifoqlMapInternal>(location, "keys", arguments, BuiltinFunctions.Keys);
                case "length": return new TypedFunctionCallExpr<IBifoqlObject>(location, "length", arguments, BuiltinFunctions.Length);
                case "max": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "max", arguments, BuiltinFunctions.Max);
                case "max_by": return new TypedFunctionCallExpr<IBifoqlArrayInternal, IBifoqlExpression>(location, "max_by", arguments, BuiltinFunctions.MaxBy);
                case "min": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "min", arguments, BuiltinFunctions.Min);
                case "min_by": return new TypedFunctionCallExpr<IBifoqlArrayInternal, IBifoqlExpression>(location, "min_by", arguments, BuiltinFunctions.MinBy);
                case "reverse": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "reverse", arguments, BuiltinFunctions.Reverse);
                case "sort": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "sort", arguments, BuiltinFunctions.Sort);
                case "sort_by": return new TypedFunctionCallExpr<IBifoqlArrayInternal, IBifoqlExpression>(location, "sort_by", arguments, BuiltinFunctions.SortBy);
                case "starts_with": return new BinaryExpr(arguments[0], "starts_with", arguments[1]);
                case "sum": return new TypedFunctionCallExpr<IBifoqlArrayInternal>(location, "sum", arguments, BuiltinFunctions.Sum);
                case "to_number": return new TypedFunctionCallExpr<IBifoqlString>(location, "to_number", arguments, BuiltinFunctions.ToNumber);
                case "to_map": return new TypedFunctionCallExpr<IBifoqlArrayInternal, IBifoqlExpression, IBifoqlExpression>(location, "to_map", arguments, BuiltinFunctions.ToMap);
                case "type": return new TypeExpr(location, arguments);
                case "unzip": return new TypedFunctionCallExpr<IBifoqlMapInternal>(location, "unzip", arguments, BuiltinFunctions.Unzip);
                case "values": return new TypedFunctionCallExpr<IBifoqlMapInternal>(location, "values", arguments, BuiltinFunctions.Values);
                case "zip": return new TypedFunctionCallExpr<IBifoqlArrayInternal, IBifoqlArrayInternal>(location, "zip", arguments, BuiltinFunctions.Zip);
                default:
                    return new ErrorExpr(location, $"Unknown function name {functionNameToken.Text}");
            }
        }

        private static Location GetLocation(Token token)
        {
            return new Location(token.LineStart, token.ColStart);
        }

        private Expr ParseIndexedLookup(Expr leftHandSide)
        {
            leftHandSide = leftHandSide ?? ParseAtomicExpr();
            if (GetToken().Kind == "(")
            {
                Match("(");

                var lookupArguments = new Dictionary<string, Expr>();

                // We don't have to have any arguments at all.
                if (GetToken().Kind != ")")
                {
                    while (true)
                    {
                        var variable = Match("ID");
                        Match(":");
                        var value = ParseExpr();

                        lookupArguments.Add(variable.Text, value);

                        if (GetToken().Kind == ")")
                        {
                            break;
                        }

                        Match(",");
                    }
                }

                Match(")");

                return new IndexedLookupExpr(leftHandSide, lookupArguments);
            }
            else
            {
                return leftHandSide;
            }
        }

        private Expr ParseIndexedLookup()
        {
            // This is super gross; why are indexed lookups treated differently from keys?
            // Why aren't keys treated like this?
            return ParseIndexedLookup(null);
        }

        private Expr ParseAtomicExpr()
        {
            var token = GetToken();

            if (token.Kind == "(")
            {
                Match("(");

                var expr = ParseExpr();

                Match(")");

                return expr;
            }
            else if (token.Kind == "@")
            {
                return ParseIdentityExpr();
            }
            else if (token.Kind == "VARIABLE")
            {
                return ParseVariable();
            }
            else if (TokenIsId(token) && (token.Text == "true" || token.Text == "false"))
            {
                return ParseLiteralBooleanExpr();
            }
            else if (TokenIsId(token) && token.Text == "null")
            {
                Match("ID");
                return new LiteralExpr(GetLocation(token), AsyncNull.Instance);
            }
            else if (TokenIsId(token) && token.Text == "undefined")
            {
                Match("ID");
                return new LiteralExpr(GetLocation(token), AsyncUndefined.Instance);
            }
            else if (TokenIsId(token))
            {
                return ParseKeyExpr(null);
            }
            else if (token.Kind == "[")
            {
                return ParseArrayExpr();
            }
            else if (token.Kind == "{")
            {
                return ParseMapProjectionExpr(null);
            }
            else if (token.Kind == "STRING")
            {
                return ParseLiteralStringExpr();
            }
            else if (token.Kind == "NUMBER")
            {
                return ParseLiteralNumberExpr();
            }
            else
            {
                return new ErrorExpr(GetLocation(token), "Expected atomic expression");
            }
        }

        private static bool TokenIsId(Token t)
        {
            return t.Kind == "ID";
        }

        private Expr ParseVariableReference( )
        {
            var dollar = Match("$");
            var next = GetToken();
            if (next.Kind == "ID")
            {
                Match("ID");
                return new VariableExpr(GetLocation(dollar), next.Text);
            }
            else
            {
                return new VariableExpr(GetLocation(dollar), "");
            }
        }

        private Expr ParseMapProjectionExpr(Expr prev)
        {
            var bracket = Match("{");

            var projections = new List<Expr>();

            while (true)
            {
                var token = GetToken();

                if (token.Kind == "}")
                {
                    Match("}");
                    break;
                }

                string id = null;
                Expr projection;

                if (token.Kind == "}")
                {
                    Match("}");
                    break;
                }

                if (token.Kind == "...")
                {
                    var ellipsis = Match("...");
                    var next = GetToken();
                    projection = new SpreadExpr(GetLocation(ellipsis), ParseExpr());
                }
                else if (token.Kind == "$")
                {
                    var dollar = Match("$");
                    var dollarLocation = GetLocation(dollar);
                    id = Match("ID").Text;
                    projection = new KeyValuePairExpr(dollarLocation, id, new VariableExpr(dollarLocation, id));
                }
                else
                {
                    var idToken = MatchAny(new [] { "STRING", "ID" });
                    var idLocation = GetLocation(idToken);
                    id = idToken.Text;
                    var curr = GetToken();
                    if (curr.Kind == ":")
                    {
                        Match(":");
                        projection = new KeyValuePairExpr(idLocation, id, ParseExpr());
                    }
                    else if (curr.Kind == "{")
                    {
                        // These are all equivalent:
                        // foo: foo | { a, b, c }
                        // foo | { a, b, c }
                        // foo { a, b, c }
                        MatchOptional("|");

                        var rhs = new ChainExpr(
                            new KeyExpr(idLocation, prev, id),
                            ParseExpr(),
                            ChainBehavior.ToMultipleIfArray);

                        projection = new KeyValuePairExpr(idLocation, id, rhs);
                    }
                    else if (curr.Kind == "[?")
                    {
                        // x[? filter] => x: x[? filter]
                        var keyExpr = new KeyExpr(idLocation, prev, id);
                        var rhs = ParseChainRemainder(keyExpr);

                        projection = new KeyValuePairExpr(idLocation, id, rhs);
                    }
                    else if (curr.Kind == "[")
                    {
                        // x[5] => x: x[5]
                        var keyExpr = new KeyExpr(idLocation, prev, id);
                        var rhs = ParseChainRemainder(keyExpr);

                        projection = new KeyValuePairExpr(idLocation, id, rhs);
                    }
                    else if (curr.Kind == "|")
                    {
                        // These are all equivalent:
                        // foo: foo | { a, b, c }
                        // foo | { a, b, c }
                        // foo { a, b, c }
                        MatchOptional("|");

                        var rhs = new ChainExpr(
                            new KeyExpr(idLocation, prev, id),
                            ParseExpr(),
                            ChainBehavior.OneToOne);

                        projection = new KeyValuePairExpr(idLocation, id, rhs);
                    }
                    else if (curr.Kind == "|<")
                    {
                        // This is another shorthand for filtring an array of objects. These are equivalent:
                        /// foo: foo |< { a, b, c }
                        /// foo |< { a, b, c}
                        Match("|<");

                        var rhs = new ChainExpr(
                            new KeyExpr(idLocation, prev, id),
                            ParseExpr(),
                            ChainBehavior.ToMultiple);

                        projection = new KeyValuePairExpr(idLocation, id, rhs);
                    }
                    else
                    {
                        projection = new KeyValuePairExpr(idLocation, id, new KeyExpr(idLocation, null, id));
                    }
                }
                
                projections.Add(projection);

                var tok = MatchAny(new [] { ",", "}"});
                if (tok.Kind == "}")
                {
                    break;
                }
            }

            return new MapProjectionExpr(GetLocation(bracket), prev, projections);
        }

        private Expr ParseLiteralStringExpr()
        {
            var literal = Match("STRING");
            var value = new AsyncString(literal.Text);

            return new LiteralExpr(GetLocation(literal), value);
        }

        private Expr ParseLiteralBooleanExpr()
        {
            var literal = Match("ID");
            var value = new AsyncBoolean(literal.Text == "true");

            return new LiteralExpr(GetLocation(literal), value);
        }

        private Expr ParseLiteralNumberExpr()
        {
            var literal = Match("NUMBER");
            var value = new AsyncNumber(double.Parse(literal.Text));

            return new LiteralExpr(GetLocation(literal), value);
        }

        private Expr ParseVariable()
        {
            var variable = Match("VARIABLE");
            return new VariableExpr(GetLocation(variable), variable.Text);
        }

        private List<Expr> ParseArgumentList()
        {
            Match("(");

            var curr = GetToken();
            if (curr.Kind == ")")
            {
                Match(")");
                return new List<Expr>();
            }

            var result = new List<Expr>();

            while (true)
            {
                result.Add(ParseExpr());

                if (GetToken().Kind != ",")
                {
                    break;
                }
                Match(",");
            }

            Match(")");

            return result;
        }

        private Expr ParseIdentityExpr()
        {
            var at = Match("@");

            return new IdentityExpr(GetLocation(at));
        }

        private Expr ParseKeyExpr(Expr prev)
        {
            MatchOptional(".");
            var token = MatchAny(new [] { "ID", "STRING" });
            return new KeyExpr(GetLocation(token), prev, token.Text);
        }

        private Expr ParseArrayExpr()
        {
            var tok = Match("[");

            var exprs = new List<Expr>();
            var first = true;

            while (GetToken().Kind != "]")
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Match(",");
                }
                if (GetToken().Kind != "]")
                {
                    Expr expr;
                    if (GetToken().Kind == "...")
                    {
                        var ellipsis = Match("...");
                        expr = new SpreadExpr(GetLocation(ellipsis), ParseExpr());
                    }
                    else
                    {
                        expr = ParseExpr();
                    }
                    
                    exprs.Add(expr);
                }
            }

            Match("]");

            return new ArrayExpr(GetLocation(tok), exprs);
        }

        private Expr ParseFilterExpr(Expr prev)
        {
            Match("[?");
            var condition = ParseExpr();
            Match("]");

            return new FilterExpr(prev, condition);
        }

        private Expr ParseIndexExpr(Expr prev)
        {
            var openBracket = Match("[");
            var next = GetToken();

            Expr projectionFilter = null;
            if (next.Kind == "STRING" && GetToken(1).Kind == "]")
            {
                Match("STRING");
                Match("]");
                projectionFilter = new KeyExpr(GetLocation(next), prev, next.Text);
            }
            else if (next.Kind == "..")
            {
                Match("..");

                next = GetToken();
                if (next.Kind == "]")
                {
                    Match("]");
                    projectionFilter = new SliceExpr(GetLocation(openBracket), prev, null, null);
                }
                else
                {
                    var upperBound = ParseExpr();
                    projectionFilter = new SliceExpr(GetLocation(openBracket), prev, null, upperBound);
                }
            }
            else
            {
                var filter = ParseExpr();
                next = GetToken();

                if (next.Kind == "..")
                {
                    Expr upperBound = null;
                    Match("..");
                    next = GetToken();
                    if (next.Kind != "]")
                    {
                        upperBound = ParseExpr();
                    }
                    Match("]");
                    projectionFilter = new SliceExpr(GetLocation(openBracket), prev, filter, upperBound);
                }
                else
                {
                    Match("]");
                    projectionFilter = new IndexExpr(GetLocation(openBracket), prev, filter);
                }
            }

            return projectionFilter;
        }

        private Token Match(string kind)
        {
            var token = GetToken();
            if (token.Kind != kind)
            {
                throw new ParseException(GetLocation(token), "expected " + kind);
            }

            _i++;

            return token;
        }

        private Token MatchAny(string[] kind)
        {
            var token = GetToken();
            if (!kind.Contains(token.Kind))
            {
                throw new ParseException(GetLocation(token), $"expected one of {string.Join(", ", kind)}");
            }

            _i++;

            return token;
        }

        private Token MatchOptional(string kind)
        {
            var token = GetToken();
            if (token.Kind != kind)
            {
                return null;
            }
            else
            {
                _i++;

                return token;
            }
        }

        private Token GetToken(int offset=0)
        {
            var i = _i + offset;
            return i >= _tokens.Count ? Token.Null : _tokens[i];
        }

        private enum ParseState
        {
            Start,
            InIndex,
            InId,
        }

        private class ParseException : BifoqlException
        {
            public Location Location { get; }
            public ParseException(Location location, string message) : base(message)
            {
                Location = location;
            }
        }
    }
}