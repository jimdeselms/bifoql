using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using Bifoql.Expressions;

namespace Bifoql.Tests
{
    public class SimplifyTests
    {
        [Fact]
        public void Simple()
        {
            RunTest(5, "5");
        }

        [Fact]
        public void Index()
        {
            RunTest("A", "['A', 'B'][0]");
        }

        [Fact]
        public void ComplexIndex()
        {
            RunTest("B", "['A', 'B'][500 - 499]");
        }

        [Fact]
        public void KeyAsIndex()
        {
            RunTest("world", "{greeting: 'hello', name: 'world'}['name']");
        }

        [Fact]
        public void ComplexKeyAsIndex()
        {
            RunTest("world", "{greeting: 'hello', name: 'world'}['na'+'me']");
        }

        [Fact]
        public void CommaAtEnd()
        {
            RunTest("world", "{name: 'world', }['name']");
        }

        [Fact]
        public void RootVariable()
        {
            TestToString("5 | $", "5 | $");
        }

        // [Fact]
        // public void Zip()
        // {
        //     var expected = new Dictionary<string, object>
        //     {
        //         ["name"] = "Jim",
        //         ["age"] = 49
        //     };

        //     RunTest(expected, "zip(['name', 'age'], ['Jim', 49]) | { name, age }");
        // }

        [Fact]
        public void EmptyDictionary()
        {
            RunTest(new Dictionary<string, object>(), "{}");
        }

        [Fact]
        public void EmptyList()
        {
            RunTest(new List<object>(), "[]");
        }

        [Fact]
        public void ListWithTrailingComma()
        {
            RunTest(new List<object>{ 5 }, "[5,]");
        }

        // [Fact]
        // public void Unzip()
        // {
        //     var expected = new object[] {
        //         new object[] { "name", "age" },
        //         new object[] { "Jim", 49 }
        //     };

        //     RunTest(expected, "unzip({ name: 'Jim', age: 49 })");
        // }

        // [Fact]
        // public void Keys()
        // {
        //     var expected = new object[] { "name", "age" };

        //     RunTest(expected, "keys({ name: 'Jim', age: 49 })");
        // }


        // [Fact]
        // public void Values()
        // {
        //     var expected = new object[] { "Jim", 49 };

        //     RunTest(expected, "values({ name: 'Jim', age: 49 })");
        // }

        [Fact]
        public void Equals()
        {
            RunTest(true, "null == null");
        }

        // [Fact]
        // public void Type()
        // {
        //     RunTest("string", "type('hi')");
        //     RunTest("number", "type(3.14)");
        //     RunTest("number", "type(5)");
        //     RunTest("null", "type(null)");
        //     RunTest("boolean", "type(true)");
        //     RunTest("boolean", "type(false)");
        //     RunTest("array", "type([])");
        //     RunTest("object", "type({})");
        // }

        [Fact]
        public void Ceil()
        {
            RunTest(5, "ceil(4.99)");
            RunTest(4, "ceil(4)");
        }

        [Fact]
        public void Floor()
        {
            RunTest(4, "floor(4.99)");
            RunTest(3, "floor(3)");
        }

        [Fact]
        public void Join()
        {
            RunTest("Hello, world, how, are, you", "join(', ', ['Hello', 'world', 'how', 'are', 'you'])");
            RunTest("one", "join('|', ['one'])");
            RunTest("", "join('X', [])");
        }

        [Fact]
        public void Count()
        {
            RunTest(5, "count('hello')");
            RunTest(3, "count([1, 2, 3])");
        }

        [Fact]
        public void Sum()
        {
            RunTest(12.34, "sum([10, 2, 0.3, 0.04])");
        }

        [Fact]
        public void MapSpread()
        {
            RunTest(ParseObj("{a: 1, b: 2}"), "{...{a: 1}, b: 2}");
        }

        [Fact]
        public void MapSpreadMerge()
        {
            RunTest(ParseObj("{a: 2}"), "{...{a: 1}, ...{a: 2}}");
        }

        [Fact]
        public void NumberToSring()
        {
            TestToString("5", "5");
        }

        [Fact]
        public void KeyToStringSimple()
        {
            TestToString("@.abc", "@.abc");
        }

        [Fact]
        public void KeyToStringInBrackets()
        {
            TestToString("@.xyz", "@['xyz']");
            TestToString("@.xyz", "@[\"xyz\"]");
            TestToString("@[\"xyz pdq\"]", "@['xyz pdq']");
            TestToString("@[\"\\\"\"]", "@['\"']");
            TestToString("@[\"\"]", "@['']");
        }

        private static void RunTest(object expected, string input)
        {
            var obj = Bifoql.Query.Compile(input, new string[0]);

            var literal = ((LiteralExpr)obj.Expr).Literal;
            var actualJson = JsonConvert.SerializeObject(literal.ToSimpleObject().Result);

            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        private static void TestToString(string expected, string parsed)
        {
            var obj = Bifoql.Query.Compile(parsed, new string[0]);
            var actual = obj.Expr.ToString();

            Assert.Equal(expected, actual);
        }

        private static object ParseObj(string json)
        {
            return JsonConvert.DeserializeObject<object>(json);
        }
    }
}
