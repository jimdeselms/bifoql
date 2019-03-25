[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Flexilex.Tests")]

namespace Bifoql.Lex
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public class Lexer
    {
        public Lexer(
            string idKind="ID", 
            string intKind="INT",
            string stringKind="STRING",
            string backtickStringKind=null,
            string charKind="CHAR",
            string floatKind="FLOAT",
            string errorKind="ERROR",
            string newlineKind="NEWLINE",
            string variableKind = "VARIABLE",

            string singleLineComment = "//",
            string multiLineCommentStart = "/*",
            string multiLineCommentEnd = "*/",
            
            bool hasFloats = true,
            bool charsMustBeOneChar = true,
            bool reservedWordsAreCaseSensitive = true,

            bool hasVariables = false,
            char variablePrefix = '$',

            IEnumerable<string> operators=null,
            IEnumerable<string> reservedWords=null,
            bool newLinesAreTokens=false)
        {
            _idKind = idKind;
            _intKind = intKind;
            _stringKind = stringKind;
            _backtickStringKind = backtickStringKind;
            _charKind = charKind;
            _floatKind = floatKind;
            _errorKind = errorKind;
            _newlineKind = newlineKind;
            _variableKind = variableKind;

            _hasFloats = hasFloats;
            _charsMustBeOneChar = charsMustBeOneChar;
            _reservedWordsAreCaseSensitive = reservedWordsAreCaseSensitive;

            _hasVariables = hasVariables;
            _variablePrefix = variablePrefix;

            _singleLineComment = singleLineComment;
            _multiLineCommentStart = multiLineCommentStart;
            _multiLineCommentEnd = multiLineCommentEnd;

            _newLinesAreTokens = newLinesAreTokens;

            _operators = new HashSet<string>(operators ?? new [] { 
                "(", ")", "[", "]", "{", "}", 
                "!", "@", "#", "$", "%", "^", "&", "*", "-", "=", "+", "`", "~", "\\", "|", "/", "?", "<", ">", ",", ".",
                "==", "!="
                });

            if (_reservedWordsAreCaseSensitive)
            {
                _reservedWords = new HashSet<string>(reservedWords ?? new string[0]);
            }
            else
            {
                _reservedWords = new HashSet<string>(
                    reservedWords ?? new string[0],
                    CaseInsensitiveComparer.Instance);
            }

            _operatorFragments = new HashSet<string>();
            _maxOperatorLength = 1;

            foreach (var op in _operators)
            {
                if (op.Length > _maxOperatorLength) _maxOperatorLength = op.Length;

                for (int i = 1; i <= op.Length; i++)
                {
                    _operatorFragments.Add(op.Substring(0, i));
                }
            }
        }

        private class CaseInsensitiveComparer : IEqualityComparer<string>
        {
            public static CaseInsensitiveComparer Instance => new CaseInsensitiveComparer();
            public bool Equals(string x, string y)
            {
                return x.ToLower() == y.ToLower();
            }

            public int GetHashCode(string obj)
            {
                return obj.ToLower().GetHashCode();
            }
        }

        public IEnumerable<Token> Parse(string s)
        {
            var withEndChar = s + (char)0;
            return Parse((IEnumerable<char>)withEndChar);
        }

        private readonly string _idKind;
        private readonly string _intKind;
        private readonly string _stringKind;
        private readonly string _backtickStringKind;
        private readonly string _charKind;
        private readonly string _floatKind;
        private readonly string _errorKind;
        private readonly string _newlineKind;
        private readonly string _variableKind;
        private readonly bool _hasFloats;

        private readonly bool _hasVariables;
        private readonly char _variablePrefix;
        private readonly bool _charsMustBeOneChar;
        private readonly bool _reservedWordsAreCaseSensitive;

        private readonly string _singleLineComment;
        private readonly string _multiLineCommentStart;
        private readonly string _multiLineCommentEnd;

        private readonly HashSet<string> _operators;
        private readonly HashSet<string> _reservedWords;
        private readonly bool _newLinesAreTokens;

        private readonly HashSet<string> _operatorFragments;

        private readonly int _maxOperatorLength;

        private enum State
        {
            Start,
            InId,
            InInteger,
            InFloat,
            InOperator,
            InString,
            InBacktickString,
            InNewline,

            InChar,
                
            InSingleLineComment,
            InMultiLineComment,
            InMultiLineCommentEnd,

            InVariable,
        }

        private IEnumerable<Token> Parse(IEnumerable<char> str)
        {
            var chars = new LookaheadEnumerator<char>(str.GetEnumerator(), _maxOperatorLength);
            
            var state = State.Start;
            var curr = "";

            int lineStart = 0;
            int colStart = 0;
            
            int line = 1;
            int col = 0;

            while (chars.MoveNext())
            {
                var c = chars.Current;
                if (c == '\r' || c == '\n')
                {
                    col = 0;
                    line++;
                }
                else
                {
                    col++;
                }

                switch (state)
                {
                    case State.Start:
                        curr = "";
                        lineStart = line;
                        colStart = col;

                        if (c == 0)
                        {
                            break;
                        }

                        if (char.IsLetter(c) || c == '_')
                        {
                            state = State.InId;
                            curr += c;
                            break;
                        }

                        if (char.IsDigit(c))
                        {
                            state = State.InInteger;
                            curr += c;
                            break;
                        }

                        if (c == _variablePrefix)
                        {
                            state = State.InVariable;
                            break;
                        }

                        if (MatchesSymbol(_singleLineComment, chars))
                        {
                            state = State.InSingleLineComment;
                            break;
                        }

                        if (MatchesSymbol(_multiLineCommentStart, chars))
                        {
                            state = State.InMultiLineComment;
                            break;
                        }
                        
                        if (c == '"')
                        {
                            state = State.InString;
                            break;
                        }

                        if (_backtickStringKind != null && c == '`')
                        {
                            state = State.InBacktickString;
                            break;
                        }

                        if (_charKind != null && c == '\'')
                        {
                            state = State.InChar;
                            break;
                        }

                        if (char.IsWhiteSpace(c))
                        {
                            if (_newLinesAreTokens)
                            {
                                if (c == '\n')
                                {
                                    yield return new Token(_newlineKind, "\n", lineStart, colStart, lineStart, colStart+1);
                                }
                                if (c == '\r')
                                {
                                    state = State.InNewline;
                                    break;
                                }
                            }
                            break;
                        }

                        if (MightBeOperator(c))
                        {
                            if (MightBeOperator(c.ToString() + chars.Values[1]))
                            {
                                state = State.InOperator;
                                curr += c;
                                break;
                            }

                            if (_operators.Contains(c.ToString()))
                            {
                                yield return new Token(c.ToString(), c.ToString(), lineStart, colStart, lineStart, colStart+1);
                                break;
                            }
                        }

                        yield return new Token(_errorKind, $"Unexpected character '{c}' ({(int)c})", lineStart, colStart, line, col);
                        state = State.Start;
                        break;

                    case State.InInteger:
                        if (!char.IsDigit(c))
                        {
                            if (_hasFloats && c == '.')
                            {
                                if (char.IsDigit(chars.Values[1]))
                                {
                                    state = State.InFloat;
                                    curr += ".";
                                    break;
                                }
                                else
                                {
                                    yield return new Token(_intKind, curr, lineStart, colStart, line, col);

                                    // TODO: Handle the case where you can have ".." or some other operator.
                                    if (_operators.Contains("." + chars.Next) 
                                        || _operators.Contains("." + chars.Next + chars.Values[2])
                                        || _operators.Contains("." + chars.Next + chars.Values[2] + chars.Values[3]))
                                    {
                                        lineStart = line;
                                        colStart = col;
                                        curr = ".";

                                        state = State.InOperator;
                                        break;
                                    }
                                    else if (_operators.Contains("."))
                                    {
                                        yield return new Token(".", ".", lineStart, col, line, col + 1);
                                        state = State.Start;
                                        break;
                                    }
                                    else
                                    {
                                        yield return new Token(_errorKind, "Unexpected token .", lineStart, colStart, lineStart, colStart + 1);
                                        state = State.Start;
                                        break;
                                    }
                                }
                            }

                            yield return new Token(_intKind, curr, lineStart, colStart, line, col);
                            state = State.Start;

                            chars.Unwind();
                            col--;

                            break;
                        }
                        else
                        {
                            curr += c;
                            break;
                        }
                    case State.InFloat:
                        if (!char.IsDigit(c))
                        {
                            yield return new Token(_floatKind, curr, lineStart, colStart, line, col);
                            state = State.Start;
                            chars.Unwind();
                            col--;
                            break;
                        }
                        else
                        {
                            curr += c;
                            break;
                        }
                    case State.InId:
                        if (c != '_' && !char.IsLetter(c) && !char.IsDigit(c))
                        {
                            state = State.Start;

                            if (_reservedWords.Contains(curr))
                            {
                                yield return new Token(curr.ToLower(), curr.ToLower(), lineStart, colStart, line, col);
                            }
                            else
                            {
                                yield return new Token(_idKind, curr, lineStart, colStart, line, col);
                            }

                            chars.Unwind();
                            col--;

                            break;
                        }
                        else
                        {
                            curr += c;
                            break;
                        }
                    case State.InVariable:
                        if(c != '_' && !char.IsLetter(c) && !char.IsDigit(c))
                        {
                            state = State.Start;

                            yield return new Token(_variableKind, curr.ToLower(), lineStart, colStart, line, col);

                            chars.Unwind();
                            col--;

                            break;
                        }
                        else
                        {
                            curr += c;
                            break;
                        }
                    case State.InOperator:
                        curr += c;

                        if (MightBeOperator(curr.ToString() + chars.Values[1]))
                        {
                            break;
                        }

                        if (_operators.Contains(curr))
                        {
                            yield return new Token(curr, curr, lineStart, colStart, line, col+1);
                            state = State.Start;
                            break;
                        }

                        yield return new Token(_errorKind, $"Unknown token {curr}", lineStart, colStart, line, col);
                        state = State.Start;
                        break;

                    case State.InString:
                    case State.InBacktickString:
                    case State.InChar:
                        char terminator;
                        string kind;
                        if (state == State.InString)
                        {
                            terminator = '"';
                            kind = _stringKind;
                        }
                        else if (state == State.InChar)
                        {
                            terminator = '\'';
                            kind = _charKind;
                        }
                        else if (state == State.InBacktickString)
                        {
                            terminator = '`';
                            kind = _backtickStringKind;
                        }
                        else
                        {
                            throw new Exception("Unexpected state");
                        }

                        if (c == terminator)
                        {
                            if (curr.Length != 1 && kind == _charKind && _charsMustBeOneChar)
                            {
                                yield return new Token(_errorKind, "Char must have exactly one character", lineStart, colStart, line, col + 1);
                            }
                            else
                            {
                                yield return new Token(kind, curr, lineStart, colStart, line, col + 1);
                            }
                            state = State.Start;
                            break;
                        }
                        else if (c == '\r' || c == '\n')
                        {
                            yield return new Token(_errorKind, "Unexpected end of line", lineStart, colStart, line, col + 1);
                            state = State.Start;
                            break;
                        }
                        else
                        {
                            curr += c;
                            break;
                        }
                    case State.InSingleLineComment:
                        if (c == '\n' || c == '\n')
                        {
                            state = State.Start;
                        }
                        break;
                    case State.InMultiLineComment:
                        if (_multiLineCommentEnd == c.ToString())
                        {
                            state = State.Start;
                            break;
                        }

                        if (_multiLineCommentEnd.StartsWith(c.ToString()))
                        {
                            state = State.InMultiLineCommentEnd;
                            curr = c.ToString();
                            break;
                        }

                        break;
                    case State.InMultiLineCommentEnd:
                        if (_multiLineCommentEnd.StartsWith(curr + c))
                        {
                            if (_multiLineCommentEnd == (curr + c))
                            {
                                state = State.Start;
                                break;
                            }
                            curr += c;
                            break;
                        }
                        else
                        {
                            curr = "";
                            state = State.InMultiLineComment;
                            break;
                        }
                    case State.InNewline:
                        // If we're here, it means that the previous characters was \r
                        if (c == '\n')
                        {
                            state = State.Start;
                            curr = "";
                            yield return new Token(_newlineKind, "\r\n", lineStart, colStart, line, col + 1);
                            break;
                        }
                        else if (c == '\r')
                        {
                            state = State.Start;
                            curr = "";
                            yield return new Token(_newlineKind, "\r", lineStart, colStart, lineStart, colStart + 1);
                            yield return new Token(_newlineKind, "\r", line, col, line, col+1);
                            break;
                        }
                        else
                        {
                            // Any other character means that we just had a \r and have to put this icharacter back.
                            state = State.Start;
                            chars.Unwind();
                            yield return new Token(_newlineKind, "\r", lineStart, colStart, lineStart, colStart+1);
                            break;
                        }
                    default:
                        yield return new Token(_errorKind, $"Unexpected state {state}", lineStart, colStart, line, col);
                        state = State.Start;
                        break;
                }
            }

            yield return new Token("EOF", "", lineStart, colStart, lineStart, colStart);
        }

        private bool MatchesSymbol(string symbol, LookaheadEnumerator<char> chars)
        {
            // Optimized for performance, not for beauty.
            switch (symbol.Length)
            {
                case 1: return symbol[0] == chars.Values[0];
                case 2: return symbol[0] == chars.Values[0] && symbol[1] == chars.Values[1];
                case 3: return symbol[0] == chars.Values[0] && symbol[1] == chars.Values[1] && symbol[2] == chars.Values[2];
                case 4: return symbol[0] == chars.Values[0] && symbol[1] == chars.Values[1] && symbol[2] == chars.Values[2] && symbol[3] == chars.Values[3];
                default: return false;
            }
        }

        private IEnumerable<char> ReadCharsFromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    foreach (var c in line)
                    {
                        yield return c;
                    }
                }
            }
        }

        private bool MightBeOperator(string s)
        {
            if (s[s.Length-1] == (char)0)
            {
                return false;
            }
            return _operatorFragments.Contains(s);
        }
        private bool MightBeOperator(char c)
        {
            return MightBeOperator(c.ToString());
        }
    }
}