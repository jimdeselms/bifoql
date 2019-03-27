using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bifoql.Tests
{
    public class ErrorTests : ExprTestBase
    {
        [Fact]
        public void SimpleObjectButNotIdentity()
        {
            RunTest(
                expected: "<error: (1, 1) key expression must be applied to array or map>", 
                input: 5, 
                query: "notIdentity");
        }

        [Fact]
        public void DivByZero()
        {
            RunTest(expected: "<error: (1, 1) division by zero>", query: "5 / 0");
        }

        [Fact]
        public void ErrorFunction()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test')"
            );
        }


        [Fact]
        public void ErrorFunctionNotString()
        {
            // TODO: We want to serialize the object here, but I don't want to depend on Newtonsoft.
            // Maybe we can do some simple serialization here, but not go too crazy.
            RunTest(
                expected: "<error: (1, 1) System.Object[]>",
//                expected: "<error: (1, 1) [1,2]>",
                query: "error([1, 2])"
            );
        }

        [Fact]
        public void MatchError()
        {
            RunTest(
                expected: "<error: (1, 6) expected one of ID, STRING>",
                query: " @ . 7"
            );
        }

        [Fact]
        public void MatchEndOfFile()
        {
            // This should really give the line/column, but apparently flexilex's eof doesnt have that.
            RunTest(
                expected: "<error: (1, 5) expected one of ID, STRING>",
                query: " @ ."
            );
        }

        [Fact]
        public void NullCoalesceWhenError()
        {
            RunTest(expected: 6, query: "{a: 1}.b ?? 6");
        }

        // These following tests verify that error propgation happens properly.
        // When an expression would act on an object that is an error, then
        // the result of the expression should just be that error; it shouldn't
        // try to, say, add 5 to the error, which would cause the error to turn into
        // a different error.
        [Fact]
        public void ErrorThenKey()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test').name"
            );
        }

        [Fact]
        public void ErrorThenIndex()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test')[0]"
            );
        }

        [Fact]
        public void ErrorThenFilter()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test')[? true]"
            );
        }

        [Fact]
        public void ErrorWithUnary()
        {
            RunTest(
                expected: "<error: (1, 2) test>",
                query: "-error('test')"
            );
        }

        [Fact]
        public void ErrorWithBinaryOnLeftSide()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test') + 9"
            );
        }

        [Fact]
        public void ErrorWithBinaryOnRightSide()
        {
            RunTest(
                expected: "<error: (1, 6) test>",
                query: "10 * error('test')"
            );
        }

        [Fact]
        public void ErrorThenChain()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test') | x"
            );
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test') |< x"
            );
        }

        [Fact]
        public void ErrorInTernary()
        {
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test') ? 1 : 2"
            );
            RunTest(
                expected: "<error: (1, 8) error if true>",
                query: "true ? error('error if true') : 2"
            );
            RunTest(
                expected: "<error: (1, 13) error if false>",
                query: "false ? 1 : error('error if false')"
            );
            RunTest(
                expected: 2,
                query: "false ? error('error if true') : 2"
            );
            RunTest(
                expected: 1,
                query: "true ? 1 : error('error if false')"
            );
        }

        [Fact]
        public void ErrorInFunction()
        {
            RunTest(
                expected: "<error: (1, 8) test>",
                query: "length(error('test'))"
            );
            RunTest(
                expected: "<error: (1, 6) test>",
                query: "join(error('test'), ['a'])"
            );
            RunTest(
                expected: "<error: (1, 11) test>",
                query: "join(',', error('test'))"
            );
        }

        [Fact]
        public void ErrorInLookup()
        {
            var input = new MockAsyncIndex("key", "fred", 20);

            RunTest(
                input: input,
                expected: "<error: (1, 8) test>",
                query: "@(key: error('test'))"
            );
            RunTest(
                expected: "<error: (1, 1) test>",
                query: "error('test')(x: 1)"
            );
        }
    }
}
