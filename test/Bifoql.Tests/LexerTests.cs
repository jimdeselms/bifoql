using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Bifoql.Lex;

namespace Bifoql.Tests
{
    public class LexerTests
    {
        [Theory]
        [InlineData("x", "ID/x:1,1,1,2")]
        [InlineData("x123_", "ID/x123_:1,1,1,6")]
        public void Id(string id, string expected) 
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(id);
            AssertToken(tokens, expected);
        }

        [Fact]
        public void AlternateIdKind() 
        {
            var lexer = new Lexer(idKind: "NAME");
            var tokens = lexer.Parse("foo");
            AssertKind(tokens, "NAME");
        }

        [Fact]
        public void TwoIdsSeparatedByWhitespace()
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse("foo bar");

            AssertToken(tokens, "ID/foo:1,1,1,4", "ID/bar:1,5,1,8");
        }

        [Fact]
        public void TwoIdsSeparatedByOperator()
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse("foo+bar");

            AssertToken(tokens, "ID/foo:1,1,1,4", "+:1,4,1,5", "ID/bar:1,5,1,8");
        }

        [Fact]
        public void Int()
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse("1 23 4");
            AssertToken(tokens, "INT/1:1,1,1,2", "INT/23:1,3,1,5", "INT/4:1,6,1,7");
        }

        [Theory]
        [InlineData("1.2 3.14159", "FLOAT/1.2:1,1,1,4 | FLOAT/3.14159:1,5,1,12")]
        [InlineData("1.", "INT/1:1,1,1,2 | .:1,2,1,3")]
        [InlineData("1..9", "INT/1:1,1,1,2 | ..:1,2,1,4 | INT/9:1,4,1,5")]
        [InlineData("1...9", "INT/1:1,1,1,2 | ...:1,2,1,5 | INT/9:1,5,1,6")]
        public void Float(string id, string expected)
        {
            var lexer = new Lexer(operators: new[] { ".", "..", "..." });
            var tokens = lexer.Parse(id);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("1.2", "INT/1:1,1,1,2 | .:1,2,1,3 | INT/2:1,3,1,4")]
        public void FloatsNotAllowed(string id, string expected)
        {
            var lexer = new Lexer(hasFloats: false);
            var tokens = lexer.Parse(id);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("1.2+3.4", "FLOAT/1.2:1,1,1,4 | +:1,4,1,5 | FLOAT/3.4:1,5,1,8")]
        public void FloatsSeparatedByOperator(string id, string expected)
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(id);
            AssertToken(tokens, expected);
        }

        [Fact]
        public void IntCanBeRenamed()
        {
            var lexer = new Lexer(intKind: "NUMBER");
            var tokens = lexer.Parse("1");
            AssertKind(tokens, "NUMBER");
            AssertText(tokens, "1");
        }
        
        [Theory]
        [InlineData("*", "*:1,1,1,2")]
        [InlineData("**", "**:1,1,1,3")]
        [InlineData("***", "***:1,1,1,4")]
        [InlineData("** *", "**:1,1,1,3 | *:1,4,1,5")]
        [InlineData("-+", "-:1,1,1,2 | +:1,2,1,3")]
        [InlineData("[][]", "[:1,1,1,2 | ]:1,2,1,3 | [:1,3,1,4 | ]:1,4,1,5")]
        public void Operators(string text, string expected)
        {
            var lexer = new Lexer(operators: new [] {"*", "**", "***", "-", "+", "[", "]"});
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("switch", "switch:1,1,1,7")]
        [InlineData("SwitCH", "ID/SwitCH:1,1,1,7")]
        [InlineData("switchy", "ID/switchy:1,1,1,8")]
        public void CaseSensitiveReservedWords(string text, string expected)
        {
            var lexer = new Lexer(reservedWords: new[] { "switch" });
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("switch", "switch:1,1,1,7")]
        [InlineData("SwitCH", "switch:1,1,1,7")]
        [InlineData("switchy", "ID/switchy:1,1,1,8")]
        public void CaseInsensitiveReservedWords(string text, string expected)
        {
            var lexer = new Lexer(reservedWords: new[] { "switch" }, reservedWordsAreCaseSensitive: false);
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }


        [Theory]
        [InlineData("\"hello\"", "STRING/hello:1,1,1,8")]
        [InlineData("\"\"", "STRING/:1,1,1,3")]
        [InlineData("\"\n", "ERROR/Unexpected end of line:1,1,2,1")]
        public void String(string text, string expected)
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("`hello`", "BACKTICK_STRING/hello:1,1,1,8")]
        [InlineData("``", "BACKTICK_STRING/:1,1,1,3")]
        [InlineData("`\n", "ERROR/Unexpected end of line:1,1,2,1")]
        public void BacktickString(string text, string expected)
        {
            var lexer = new Lexer(backtickStringKind: "BACKTICK_STRING");
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("'a'", "CHAR/a:1,1,1,4")]
        [InlineData("''", "ERROR/Char must have exactly one character:1,1,1,3")]
        [InlineData("'aa'", "ERROR/Char must have exactly one character:1,1,1,5")]
        public void Char(string text, string expected)
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("''", "STRING/:1,1,1,3")]
        [InlineData("'a'", "STRING/a:1,1,1,4")]
        [InlineData("'aa'", "STRING/aa:1,1,1,5")]
        public void CharAsString(string text, string expected)
        {
            var lexer = new Lexer(charKind: "STRING", charsMustBeOneChar: false);
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("'a'=='b'", "CHAR/a:1,1,1,4 | ==:1,4,1,6 | CHAR/b:1,6,1,9")]
        public void CharsSeparatedByOperator(string text, string expected)
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Fact]
        public void SingleLineComment()
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse("hello // comment\nworld");
            AssertToken(tokens, "ID/hello:1,1,1,6 | ID/world:2,1,2,6");
        }

        // [Fact]
        // public void SingleLineCommentFourCharacters()
        // {
        //     var lexer = new Lexer(singleLineComment: "**--");
        //     var tokens = lexer.Parse("hello **-- comment\nworld");
        //     AssertToken(tokens, "ID/hello:1,1,1,6 | ID/world:2,1,2,6");
        // }

        [Fact]
        public void SingleLineCommentThreeCharacters()
        {
            var lexer = new Lexer(singleLineComment: "---");
            var tokens = lexer.Parse("hello --- comment\nworld");
            AssertToken(tokens, "ID/hello:1,1,1,6 | ID/world:2,1,2,6");
        }

        [Fact]
        public void SingleLineCommentSingleCharacter()
        {
            var lexer = new Lexer(singleLineComment: "#");
            var tokens = lexer.Parse("hello # comment\nworld");
            AssertToken(tokens, "ID/hello:1,1,1,6 | ID/world:2,1,2,6");
        }

        [Theory]
        [InlineData("hello /*\n*/world", "ID/hello:1,1,1,6 | ID/world:2,3,2,8")]
        [InlineData("hello /*\n /* * / /* \n*/world", "ID/hello:1,1,1,6 | ID/world:3,3,3,8")]
        public void MultilineComment(string text, string expected)
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        // [Theory]
        // [InlineData("hello <!--\n!-->world", "ID/hello:1,1,1,6 | ID/world:2,5,2,10")]
        // [InlineData("hello <!--\n ! !- !-- <!-- \n!-->world", "ID/hello:1,1,1,6 | ID/world:3,5,3,10")]
        // public void MultilineCommentLongTokens(string text, string expected)
        // {
        //     var lexer = new Lexer(multiLineCommentStart: "<!--", multiLineCommentEnd: "!-->");
        //     var tokens = lexer.Parse(text);
        //     AssertToken(tokens, expected);
        // }


        [Theory]
        [InlineData("[][]", "[:1,1,1,2 | ]:1,2,1,3 | [:1,3,1,4 | ]:1,4,1,5")]
        public void ChainOfOperators(string text, string expected)
        {
            var lexer = new Lexer();
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        [Theory]
        [InlineData("this\nthat", "ID/this | NEWLINE/\n | ID/that")]
        [InlineData("this\n\nthat", "ID/this | NEWLINE/\n | NEWLINE/\n | ID/that")]
        [InlineData("this\n\nthat", "ID/this | NEWLINE/\n | NEWLINE/\n | ID/that")]
        [InlineData("this\r\nthat", "ID/this | NEWLINE/\r\n | ID/that")]
        [InlineData("this\r\rthat", "ID/this | NEWLINE/\r | NEWLINE/\r | ID/that")]
        public void NewLinesCanBeTokens(string text, string expected)
        {
            var lexer = new Lexer(newLinesAreTokens: true);
            var tokens = lexer.Parse(text);
            AssertToken(tokens, expected);
        }

        private void AssertKind(IEnumerable<Token> tokens, params string[] kinds)
        {
            AssertToken(tokens, t => t.Kind, kinds);
        }

        private void AssertText(IEnumerable<Token> tokens, params string[] texts)
        {
            AssertToken(tokens, t => t.Text, texts);
        }

        private void AssertToken(IEnumerable<Token> tokens, params string[] expected)
        {
            expected = expected.SelectMany(e => e.Split('|')).Select(e => e.Trim(' ')).ToArray();

            var tokList = tokens.ToList();

            Assert.Equal(expected.Length + 1, tokList.Count);

            for (int i = 0; i < expected.Length; i++)
            {
                AssertTokenMatches(tokList[i], expected[i]);
            }
        }

        private void AssertTokenMatches(Token t, string expected)
        {
            int colonIdx = expected.IndexOf(':');
            var kindAndText = colonIdx == -1 ? expected : expected.Substring(0, colonIdx);

            int slashIdx = kindAndText.IndexOf('/');
            var kind = slashIdx == -1 ? kindAndText : kindAndText.Substring(0, slashIdx);
            var text = slashIdx == -1 ? kindAndText : kindAndText.Substring(slashIdx + 1);
            
            Assert.Equal(kind, t.Kind);
            Assert.Equal(text, t.Text);

            if (colonIdx > -1)
            {
                var expectedPos = expected.Substring(colonIdx + 1);
                var actualPos = $"{t.LineStart},{t.ColStart},{t.LineEnd},{t.ColEnd}";

                Assert.Equal(expectedPos, actualPos);
            }
        }

        private void AssertToken<T>(IEnumerable<Token> tokens, Func<Token, T> extractFn, params T[] expected)
        {
            var tokList = tokens.ToList();

            Assert.Equal(expected.Length + 1, tokList.Count);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], extractFn(tokList[i]));
            }

            Assert.Equal("EOF", tokList[expected.Length].Kind);
        }
    }
}
