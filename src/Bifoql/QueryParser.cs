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

        internal QueryParser(IReadOnlyDictionary<string, CustomFunction> customFunctions)
        {
            _customFunctions = customFunctions ?? new Dictionary<string, CustomFunction>();
        }

        internal Expr Parse(string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return new IdentityExpr(new Location(1, 1));
                }

                var lexer = new Lexer(
                    operators: new [] { 
                        "|", "|<", "&", ".", "(", "[", "{", "}", "]", ")", ".", "@", "*", 
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

                int i = 0;

                return ParseExpr(tokens, ref i);
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

        private Expr ParseExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            return ParseAssignment(tokens, ref i);
        }

        private Expr ParseAssignment(IReadOnlyList<Token> tokens, ref int i)
        {
            var token = GetToken(tokens, i);
            if (GetToken(tokens, i).Kind == "VARIABLE" && GetToken(tokens, i+1).Kind == "=")
            {
                var variable = Match(tokens, "VARIABLE", ref i);
                Match(tokens, "=", ref i);

                var variableValue = ParsePipe(tokens, ref i);

                Match(tokens, ";", ref i);

                var pipedInto = ParseExpr(tokens, ref i);

                return new AssignmentExpr(GetLocation(token), variable.Text, variableValue, pipedInto);
            }
            else
            {
                return ParsePipe(tokens, ref i);
            }
        }

        private Expr ParsePipe(IReadOnlyList<Token> tokens, ref int i)
        {
            var expr = ParseTernaryExpr(tokens, ref i);

            var token = GetToken(tokens, i);

            if (token.Kind == "|")
            {
                Match(tokens, "|", ref i);

                var next = ParsePipe(tokens, i: ref i);
                return new ChainExpr(expr, next, toMultiple: false);
            }
            else if (token.Kind == "|<")
            {
                Match(tokens, "|<", ref i);

                var next = ParsePipe(tokens, i: ref i);
                return new ChainExpr(expr, next, toMultiple: true);
            }
            else
            {
                return expr;
            }
        }

        private Expr ParseTernaryExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var expr = ParseNullCoalescingExpr(tokens, ref i);

            if (GetToken(tokens, i).Kind == "?")
            {
                Match(tokens, "?", ref i);

                var ifTrue = ParseTernaryExpr(tokens, ref i);
                Match(tokens, ":", ref i);
                var ifFalse = ParseTernaryExpr(tokens, ref i);

                return new TernaryExpr(expr, ifTrue, ifFalse);
            }
            else
            {
                return expr;
            }
        }

        private Expr ParseNullCoalescingExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseOrExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "??")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseNullCoalescingExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseOrExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseAndExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "||")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseOrExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseAndExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseEqualityExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "&&")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseAndExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseEqualityExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseInequalityExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "==" || token.Kind == "!=")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseEqualityExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseInequalityExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseAdditiveExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "<" || token.Kind == ">" || token.Kind == "<=" || token.Kind == ">=")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseInequalityExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseAdditiveExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseMultiplicativeExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "+" || token.Kind == "-")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseAdditiveExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseMultiplicativeExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var lhs = ParseUnaryExpr(tokens, ref i);

            var token = GetToken(tokens, i);
            if (token.Kind == "*" || token.Kind == "/" || token.Kind == "%")
            {
                Match(tokens, token.Kind, ref i);

                var rhs = ParseMultiplicativeExpr(tokens, ref i);

                return new BinaryExpr(lhs, token.Kind, rhs);
            }
            else
            {
                return lhs;
            }
        }

        private Expr ParseUnaryExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var token = GetToken(tokens, i);
            if (token.Kind == "&")
            {
                Match(tokens, "&", ref i);

                var innerExpression = ParseUnaryExpr(tokens, ref i);
                return new ExpressionExpr(innerExpression);
            }
            else if (token.Kind == "-")
            {
                Match(tokens, "-", ref i);

                var innerExpression = ParseUnaryExpr(tokens, ref i);
                return new UnaryExpr(GetLocation(token), "-", innerExpression);
            }
            else
            {
                return ParseChain(tokens, ref i);
            }
        }

        private Expr ParseChain(IReadOnlyList<Token> tokens, ref int i)
        {
            var expr = ParseFunctionCallExpr(tokens, ref i);

            var peek = GetToken(tokens, i);
            if (peek.Kind == "." || peek.Kind == "[" || peek.Kind == "(")
            {
                var next = ParseChainRemainder(tokens, ref i);
                return new ChainExpr(expr, next, toMultiple: false);
            }
            else
            {
                return expr;
            }
        }

        private Expr ParseChainRemainder(IReadOnlyList<Token> tokens, ref int i)
        {
            var token = GetToken(tokens, i);
            Expr first = null;
            if (token.Kind == ".")
            {
                Match(tokens, ".", ref i);
                first = ParseKeyExpr(tokens, ref i);
            }
            else if (token.Kind == "[")
            {
                first = ParseIndexExpr(tokens, ref i);
            }
            else if (token.Kind == "(")
            {
                // Super gross; in retrospect, all expressions should just be self-contained, or this
                // should at least work the same as the other chain expressions.
                first = ParseIndexedLookup(tokens, new IdentityExpr(GetLocation(token)), ref i);
            }

            var nextToken = GetToken(tokens, i);
            if (nextToken.Kind == "." || nextToken.Kind == "[" || nextToken.Kind == "(")
            {
                return new ChainExpr(first, ParseChainRemainder(tokens, ref i), toMultiple: false);
            }
            else
            {
                return first;
            }
        }

        private static bool NextIsChainAtomicExpr(IReadOnlyList<Token> tokens, int i)
        {
            // Does the next token tell us that we're continuing a chain expression?
            var peek = GetToken(tokens, i);
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
            "eval",
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

        private Expr ParseFunctionCallExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var id = GetToken(tokens, i);
            if (id.Kind != "ID" || (!_builtinFunctionNames.Contains(id.Text) && !_customFunctions.ContainsKey(id.Text))) 
            {
                return ParseIndexedLookup(tokens, ref i);
            }

            var functionNameToken = Match(tokens, "ID", ref i);
            var arguments = ParseArgumentList(tokens, ref i).ToList();

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
                case "avg": return new TypedFunctionCallExpr<IBifoqlArray>(location, "avg", arguments, BuiltinFunctions.Avg);
                case "ceil": return new TypedFunctionCallExpr<IBifoqlNumber>(location, "ceil", arguments, BuiltinFunctions.Ceil);
                case "contains": return new BinaryExpr(arguments[0], "contains", arguments[1]);
                case "distinct": return new TypedFunctionCallExpr<IBifoqlArray>(location, "distinct", arguments, BuiltinFunctions.Distinct);
                case "ends_with": return new BinaryExpr(arguments[0], "ends_with", arguments[1]);
                case "error": return new ErrorFunctionExpr(location, arguments);
                case "eval": return new TypedFunctionCallExpr<IBifoqlExpression>(location, "eval", arguments, BuiltinFunctions.Eval);
                case "flatten": return new TypedFunctionCallExpr<IBifoqlArray>(location, "flatten", arguments, BuiltinFunctions.Flatten);
                case "floor": return new TypedFunctionCallExpr<IBifoqlNumber>(location, "floor", arguments, BuiltinFunctions.Floor);
                case "if_error": return new IfErrorExpr(location, arguments);
                case "join": return new TypedFunctionCallExpr<IBifoqlString, IBifoqlArray>(location, "join", arguments, BuiltinFunctions.Join);
                case "keys": return new TypedFunctionCallExpr<IBifoqlMap>(location, "keys", arguments, BuiltinFunctions.Keys);
                case "length": return new TypedFunctionCallExpr<IBifoqlObject>(location, "length", arguments, BuiltinFunctions.Length);
                case "max": return new TypedFunctionCallExpr<IBifoqlArray>(location, "max", arguments, BuiltinFunctions.Max);
                case "max_by": return new TypedFunctionCallExpr<IBifoqlArray, IBifoqlExpression>(location, "max_by", arguments, BuiltinFunctions.MaxBy);
                case "min": return new TypedFunctionCallExpr<IBifoqlArray>(location, "min", arguments, BuiltinFunctions.Min);
                case "min_by": return new TypedFunctionCallExpr<IBifoqlArray, IBifoqlExpression>(location, "min_by", arguments, BuiltinFunctions.MinBy);
                case "reverse": return new TypedFunctionCallExpr<IBifoqlArray>(location, "reverse", arguments, BuiltinFunctions.Reverse);
                case "sort": return new TypedFunctionCallExpr<IBifoqlArray>(location, "sort", arguments, BuiltinFunctions.Sort);
                case "sort_by": return new TypedFunctionCallExpr<IBifoqlArray, IBifoqlExpression>(location, "sort_by", arguments, BuiltinFunctions.SortBy);
                case "starts_with": return new BinaryExpr(arguments[0], "starts_with", arguments[1]);
                case "sum": return new TypedFunctionCallExpr<IBifoqlArray>(location, "sum", arguments, BuiltinFunctions.Sum);
                case "to_number": return new TypedFunctionCallExpr<IBifoqlString>(location, "to_number", arguments, BuiltinFunctions.ToNumber);
                case "to_map": return new TypedFunctionCallExpr<IBifoqlArray, IBifoqlExpression, IBifoqlExpression>(location, "to_map", arguments, BuiltinFunctions.ToMap);
                case "type": return new TypeExpr(location, arguments);
                case "unzip": return new TypedFunctionCallExpr<IBifoqlMap>(location, "unzip", arguments, BuiltinFunctions.Unzip);
                case "values": return new TypedFunctionCallExpr<IBifoqlMap>(location, "values", arguments, BuiltinFunctions.Values);
                case "zip": return new TypedFunctionCallExpr<IBifoqlArray, IBifoqlArray>(location, "zip", arguments, BuiltinFunctions.Zip);
                default:
                    return new ErrorExpr(location, $"Unknown function name {functionNameToken.Text}");
            }
        }

        private static Location GetLocation(Token token)
        {
            return new Location(token.LineStart, token.ColStart);
        }

        private Expr ParseIndexedLookup(IReadOnlyList<Token> tokens, Expr leftHandSide, ref int i)
        {
            leftHandSide = leftHandSide ?? ParseAtomicExpr(tokens, ref i);
            if (GetToken(tokens, i).Kind == "(")
            {
                Match(tokens, "(", ref i);

                var lookupArguments = new Dictionary<string, Expr>();

                while (true)
                {
                    var variable = Match(tokens, "ID", ref i);
                    Match(tokens, ":", ref i);
                    var value = ParseExpr(tokens, ref i);

                    lookupArguments.Add(variable.Text, value);

                    if (GetToken(tokens, i).Kind == ")")
                    {
                        break;
                    }

                    Match(tokens, ",", ref i);
                }

                Match(tokens, ")", ref i);

                return new IndexedLookupExpr(leftHandSide, lookupArguments);
            }
            else
            {
                return leftHandSide;
            }
        }

        private Expr ParseIndexedLookup(IReadOnlyList<Token> tokens, ref int i)
        {
            // This is super gross; why are indexed lookups treated differently from keys?
            // Why aren't keys treated like this?
            return ParseIndexedLookup(tokens, null, ref i);
        }

        private Expr ParseAtomicExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var token = GetToken(tokens, i);

            if (token.Kind == "(")
            {
                Match(tokens, "(", ref i);

                var expr = ParseExpr(tokens, ref i);

                Match(tokens, ")", ref i);

                return expr;
            }
            else if (token.Kind == "@")
            {
                return ParseIdentityExpr(tokens, ref i);
            }
            else if (token.Kind == "VARIABLE")
            {
                return ParseVariable(tokens, ref i);
            }
            else if (TokenIsId(token) && (token.Text == "true" || token.Text == "false"))
            {
                return ParseLiteralBooleanExpr(tokens, ref i);
            }
            else if (TokenIsId(token) && token.Text == "null")
            {
                Match(tokens, "ID", ref i);
                return new LiteralExpr(GetLocation(token), AsyncNull.Instance);
            }
            else if (TokenIsId(token))
            {
                return ParseKeyExpr(tokens, ref i);
            }
            else if (token.Kind == "[")
            {
                return ParseArrayExpr(tokens, ref i);
            }
            else if (token.Kind == "{")
            {
                return ParseMapProjectionExpr(tokens, ref i);
            }
            else if (token.Kind == "STRING")
            {
                return ParseLiteralStringExpr(tokens, ref i);
            }
            else if (token.Kind == "NUMBER")
            {
                return ParseLiteralNumberExpr(tokens, ref i);
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

        private static Expr ParseVariableReference(IReadOnlyList<Token> tokens, ref int i )
        {
            var dollar = Match(tokens, "$", ref i);
            var next = GetToken(tokens, i);
            if (next.Kind == "ID")
            {
                Match(tokens, "ID", ref i);
                return new VariableExpr(GetLocation(dollar), next.Text);
            }
            else
            {
                return new VariableExpr(GetLocation(dollar), "");
            }
        }

        private Expr ParseMapProjectionExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var bracket = Match(tokens, "{", ref i);

            var projections = new List<Expr>();

            while (true)
            {
                var token = GetToken(tokens, i);

                if (token.Kind == "}")
                {
                    Match(tokens, "}", ref i);
                    break;
                }

                string id = null;
                Expr projection;

                if (token.Kind == "}")
                {
                    Match(tokens, "}", ref i);
                    break;
                }

                if (token.Kind == "...")
                {
                    var ellipsis = Match(tokens, "...", ref i);
                    var next = GetToken(tokens, i);
                    projection = new SpreadExpr(GetLocation(ellipsis), ParseExpr(tokens, ref i));
                }
                else if (token.Kind == "$")
                {
                    var dollar = Match(tokens, "$", ref i);
                    var dollarLocation = GetLocation(dollar);
                    id = Match(tokens, "ID", ref i).Text;
                    projection = new KeyValuePairExpr(dollarLocation, id, new VariableExpr(dollarLocation, id));
                }
                else
                {
                    var idToken = MatchAny(tokens, new [] { "STRING", "ID" }, ref i);
                    id = idToken.Text;
                    var curr = GetToken(tokens, i);
                    if (curr.Kind == ":")
                    {
                        Match(tokens, ":", ref i);
                        projection = new KeyValuePairExpr(GetLocation(idToken), id, ParseExpr(tokens, ref i));
                    }
                    else
                    {
                        projection = new KeyValuePairExpr(GetLocation(idToken), id, new KeyExpr(GetLocation(idToken), id));
                    }
                }

                
                projections.Add(projection);

                var tok = MatchAny(tokens, new [] { ",", "}"}, ref i);
                if (tok.Kind == "}")
                {
                    break;
                }
            }

            return new MapProjectionExpr(GetLocation(bracket), projections);
        }

        private static Expr ParseLiteralStringExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var literal = Match(tokens, "STRING", ref i);
            var value = new AsyncString(literal.Text);

            return new LiteralExpr(GetLocation(literal), value);
        }

        private static Expr ParseLiteralBooleanExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var literal = Match(tokens, "ID", ref i);
            var value = new AsyncBoolean(literal.Text == "true");

            return new LiteralExpr(GetLocation(literal), value);
        }

        private static Expr ParseLiteralNumberExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var literal = Match(tokens, "NUMBER", ref i);
            var value = new AsyncNumber(double.Parse(literal.Text));

            return new LiteralExpr(GetLocation(literal), value);
        }

        private static Expr ParseVariable(IReadOnlyList<Token> tokens, ref int i)
        {
            var variable = Match(tokens, "VARIABLE", ref i);
            return new VariableExpr(GetLocation(variable), variable.Text);
        }

        private List<Expr> ParseArgumentList(IReadOnlyList<Token> tokens, ref int i)
        {
            Match(tokens, "(", ref i);

            var curr = GetToken(tokens, i);
            if (curr.Kind == ")")
            {
                Match(tokens, ")", ref i);
                return new List<Expr>();
            }

            var result = new List<Expr>();

            while (true)
            {
                result.Add(ParseExpr(tokens, ref i));

                if (GetToken(tokens, i).Kind != ",")
                {
                    break;
                }
                Match(tokens, ",", ref i);
            }

            Match(tokens, ")", ref i);

            return result;
        }

        private static Expr ParseIdentityExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var at = Match(tokens, "@", ref i);

            return new IdentityExpr(GetLocation(at));
        }

        private static Expr ParseKeyExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            MatchOptional(tokens, ".", ref i);
            var token = MatchAny(tokens, new [] { "ID", "STRING" }, ref i);
            return new KeyExpr(GetLocation(token), token.Text);
        }

        private Expr ParseArrayExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var tok = Match(tokens, "[", ref i);

            var exprs = new List<Expr>();
            var first = true;

            while (GetToken(tokens, i).Kind != "]")
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Match(tokens, ",", ref i);
                }
                if (GetToken(tokens, i).Kind != "]")
                {
                    Expr expr;
                    if (GetToken(tokens, i).Kind == "...")
                    {
                        var ellipsis = Match(tokens, "...", ref i);
                        expr = new SpreadExpr(GetLocation(ellipsis), ParseExpr(tokens, ref i));
                    }
                    else
                    {
                        expr = ParseExpr(tokens, ref i);
                    }
                    
                    exprs.Add(expr);
                }
            }

            Match(tokens, "]", ref i);

            return new ArrayExpr(GetLocation(tok), exprs);
        }

        private Expr ParseFilterExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var condition = ParseExpr(tokens, ref i);
            return new FilterExpr(condition);
        }

        private Expr ParseIndexExpr(IReadOnlyList<Token> tokens, ref int i)
        {
            var openBracket = Match(tokens, "[", ref i);

            var next = GetToken(tokens, i);
            Expr projectionFilter = null;
            if (next.Kind == "STRING" && GetToken(tokens, i + 1).Kind == "]")
            {
                Match(tokens, "STRING", ref i);
                Match(tokens, "]", ref i);
                projectionFilter = new KeyExpr(GetLocation(next), next.Text);
            }
            else if (next.Kind == "..")
            {
                Match(tokens, "..", ref i);

                next = GetToken(tokens, i);
                if (next.Kind == "]")
                {
                    Match(tokens, "]", ref i);
                    projectionFilter = new SliceExpr(GetLocation(openBracket), null, null);
                }
                else
                {
                    var upperBound = ParseExpr(tokens, ref i);
                    projectionFilter = new SliceExpr(GetLocation(openBracket), null, upperBound);
                }
            }
            else
            {
                var filter = ParseExpr(tokens, ref i);
                next = GetToken(tokens, i);

                if (next.Kind == "..")
                {
                    Expr upperBound = null;
                    Match(tokens, "..", ref i);
                    next = GetToken(tokens, i);
                    if (next.Kind != "]")
                    {
                        upperBound = ParseExpr(tokens, ref i);
                    }
                    Match(tokens, "]", ref i);
                    projectionFilter = new SliceExpr(GetLocation(openBracket), filter, upperBound);
                }
                else
                {
                    Match(tokens, "]", ref i);
                    projectionFilter = new FilterExpr(filter);
                }
            }

            return projectionFilter;
        }

        private static Token Match(IReadOnlyList<Token> tokens, string kind, ref int i)
        {
            var token = GetToken(tokens, i);
            if (token.Kind != kind)
            {
                throw new ParseException(GetLocation(token), "expected " + kind);
            }

            i++;

            return token;
        }

        private static Token MatchAny(IReadOnlyList<Token> tokens, string[] kind, ref int i)
        {
            var token = GetToken(tokens, i);
            if (!kind.Contains(token.Kind))
            {
                throw new ParseException(GetLocation(token), $"expected one of {string.Join(", ", kind)}");
            }

            i++;

            return token;
        }

        private static Token MatchOptional(IReadOnlyList<Token> tokens, string kind, ref int i)
        {
            var token = GetToken(tokens, i);
            if (token.Kind != kind)
            {
                return null;
            }
            else
            {
                i++;

                return token;
            }
        }

        private static Token GetToken(IReadOnlyList<Token> tokens, int i)
        {
            return i >= tokens.Count ? Token.Null : tokens[i];
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