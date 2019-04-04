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
    public class DefaultValueTests : ExprTestBase
    {
        [Fact]
        public void IndexWithDefaultValueTest()
        {
            RunTest(
                input: new { foo = new IndexWithDefaultValue("HI") },
                expected: "HI",
                query: "foo");
            RunTest(
                input: new { foo = new IndexWithDefaultValue("HI") },
                expected: "y",
                query: "foo(id: 'x')");
        }

        [Fact]
        public void IndexWithoutDefaultValueAssumesZeroParametersTest()
        {
            RunTest(
                input: new { foo = new IndexWithoutDefaultValue("didntPassX") },
                expected: "didntPassX",
                query: "foo");
            RunTest(
                input: new { foo = new IndexWithoutDefaultValue("didntPassX") },
                expected: "passedX",
                query: "foo(id: 'x')");
        }

        [Fact]
        public void IndexWithComplexDefaultValueTest()
        {
            RunTest(
                input: new { foo = new IndexWithDefaultValue(new { a = 1, b = 2 }) },
                expected: 2,
                query: "foo.b");
            RunTest(
                input: new { foo = new IndexWithDefaultValue( new { a = 1, b = 2 }) },
                expected: "y",
                query: "foo(id: 'x')");
        }

        [Fact]
        public void MapWithDefaultValueTest()
        {
            RunTest(
                input: new MapWithDefaultValue(),
                expected: 7,
                query: "@");
            RunTest(
                input: new MapWithDefaultValue(),
                expected: 5,
                query: "@.x");
        }
        
        [Fact]
        public void LookupWithDefaultValueTest()
        {
            RunTest(
                input: new LookupWithDefaultValue(123),
                expected: 123,
                query: "@");
            RunTest(
                input: new LookupWithDefaultValue("howdy"),
                expected: "howdy",
                query: "@.x");
        }

        [Fact]
        public void PocoWithDefaultValue()
        {
            RunTest(
                input: new MyPoco(),
                expected: 999,
                query: "@");
            RunTest(
                input: new MyPoco(),
                expected: "Bill",
                query: "@.Name");
        }

        [Fact]
        public void DefaultFromIndexBeforeKey()
        {
            RunTest(
                input: new IndexWithoutDefaultValue(new { a = 'b'}),
                expected: "b",
                query: "@.a");
            RunTest(
                input: new IndexWithDefaultValue(new { a = 'b'}),
                expected: "b",
                query: "@.a");
        }

        [Fact]
        public void DefaultFromIndexAfterBeforeArrayIndex()
        {
            RunTest(
                input: new IndexWithoutDefaultValue(new [] { 5 }),
                expected: 5,
                query: "@[0]");
            RunTest(
                input: new IndexWithDefaultValue(new [] { 6 }),
                expected: 6,
                query: "@[0]");
        }

        [Fact]
        public void DefaultFromIndexAfterBeforeFilter()
        {
            RunTest(
                input: new IndexWithoutDefaultValue(new [] { new { a = 5} }),
                expected: 5,
                query: "@[? a == 5][0].a");
            RunTest(
                input: new IndexWithDefaultValue(new [] { new { a = 6} }),
                expected: 6,
                query: "@[? a == 6][0].a");
        }


        [Fact]
        public void DefaultFromIndexAfterBeforeChain()
        {
            RunTest(
                input: new IndexWithDefaultValue(new { a = "hello"}),
                expected: "hello",
                query: "@ | a");
            RunTest(
                input: new IndexWithoutDefaultValue(new { a = "hello"}),
                expected: "hello",
                query: "@ | a");
        }

        [Fact]
        public void DefaultFromIndexAfterBeforeMultiChain()
        {
            RunTest(
                input: new IndexWithDefaultValue(new [] { new { a = "hello"}, new { a = "goodbye" }}),
                expected: new [] { "hello", "goodbye" },
                query: "@ |< a");
            RunTest(
                input: new IndexWithoutDefaultValue(new [] { new { a = "hello"}, new { a = "goodbye" }}),
                expected: new [] { "hello", "goodbye" },
                query: "@ |< a");
        }

        [Fact]
        public void DefaultFromIndexInBinaryExpr()
        {
            RunTest(
                input: new IndexWithDefaultValue(5),
                expected: 10,
                query: "@ * 2");
            RunTest(
                input: new IndexWithDefaultValue(5),
                expected: 10,
                query: "2 * @");
            RunTest(
                input: new IndexWithDefaultValue(true),
                expected: true,
                query: "@ && true");
            RunTest(
                input: new IndexWithDefaultValue(true),
                expected: true,
                query: "false || @");
            RunTest(
                input: new IndexWithDefaultValue(new { x= 5 }),
                expected: 10,
                query: "@.x * 2");
            RunTest(
                input: new IndexWithDefaultValue(new { x= 5 }),
                expected: 10,
                query: "2 * @.x");
            RunTest(
                input: new IndexWithDefaultValue(new { x= true }),
                expected: true,
                query: "@.x && true");
            RunTest(
                input: new IndexWithDefaultValue(new { x= true }),
                expected: true,
                query: "false || @.x");
        }

        [Fact]
        public void DefaultFromIndexInUnaryExpr()
        {
            RunTest(
                input: false,
                expected: true,
                query: "!@");
            RunTest(
                input: new IndexWithDefaultValue(100),
                expected: -100,
                query: "-(@)");
            RunTest(
                input: new IndexWithDefaultValue(new { a = false }),
                expected: true,
                query: "!(@.a)");
            RunTest(
                input: new IndexWithDefaultValue(new { a = 100 }),
                expected: -100,
                query: "-(@.a)");
        }

        private class MyPoco : IDefaultValueSync
        {
            public string Name { get { return "Bill"; }}

            public object GetDefaultValue()
            {
                return 999;
            }
        }
        
        internal class IndexWithDefaultValue : IBifoqlIndexSync, IDefaultValueSync
        {
            private readonly object _value;

            public IndexWithDefaultValue(object value)
            {
                _value = value;
            }
            public object GetDefaultValue()
            {
                return _value;
            }

            public object Lookup(IIndexArgumentList args)
            {
                if (args.TryGetStringParameter("id") == "x") return "y";
                return null;
            }
        }

        internal class IndexWithoutDefaultValue : IBifoqlIndexSync
        {
            private readonly object _value;

            public IndexWithoutDefaultValue(object value)
            {
                _value = value;
            }
            public object Lookup(IIndexArgumentList args)
            {
                if (args.TryGetStringParameter("id") == "x") return "passedX";
                return _value;
            }
        }

        protected class IndexWithComplexDefaultValue : IBifoqlIndexSync, IDefaultValueSync
        {
            public object GetDefaultValue()
            {
                return new { a = 1, b = 2 };
            }

            public object Lookup(IIndexArgumentList args)
            {
                if (args.TryGetStringParameter("id") == "x") return "y";
                return null;
            }
        }

        protected class MapWithDefaultValue : IBifoqlMapSync, IDefaultValueSync
        {
            public IReadOnlyDictionary<string, Func<object>> Items => new Dictionary<string, Func<object>>()
            {
                ["x"] = () => 5
            };

            public object GetDefaultValue()
            {
                return 7;
            }
        }

        protected class LookupWithDefaultValue : IBifoqlLookupSync, IDefaultValueSync
        {
            private readonly object _defaultValue;

            internal LookupWithDefaultValue(object defaultValue)
            {
                _defaultValue = defaultValue;
            }
            public object GetDefaultValue()
            {
                return _defaultValue;
            }

            public bool TryGetValue(string key, out Func<object> result)
            {
                if (key == "x")
                {
                    result = () => "howdy";
                    return true;
                }
                result = null;
                return false;
            }
        }
    }
}
