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
                input: new { foo = new IndexWithDefaultValue() },
                expected: "HI",
                query: "foo");
            RunTest(
                input: new { foo = new IndexWithDefaultValue() },
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
                input: new LookupWithDefaultValue(),
                expected: 123,
                query: "@");
            RunTest(
                input: new LookupWithDefaultValue(),
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

        private class MyPoco : IDefaultValueSync
        {
            public string Name { get { return "Bill"; }}

            public object GetDefaultValue()
            {
                return 999;
            }
        }
        
        protected class IndexWithDefaultValue : IBifoqlIndexSync, IDefaultValueSync
        {
            public object GetDefaultValue()
            {
                return "HI";
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
            public object GetDefaultValue()
            {
                return 123;
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
