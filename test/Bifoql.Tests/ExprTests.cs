using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bifoql.Tests
{
    public class ExprTests
    {
        [Fact]
        public void SimpleIdentity()
        {
            RunTest(
                expected: "5", 
                input: "5", 
                query: "");
        }

        [Fact]
        public void SimpleObjectButNotIdentity()
        {
            RunTest(
                expected: "<error: (1, 1) key expression must be applied to array or map>", 
                input: 5, 
                query: "notIdentity");
        }

        [Fact]
        public void GetNthItemInArray()
        {
            RunTest(
                expected: 3, 
                input: new [] {1,2,3,4,5}, 
                query: "@[2]");
        }

        [Fact]
        public void Slices()
        {
            RunTest(expected: new [] { 2, 3 }, input: new [] { 0, 1, 2, 3, 4, 5 }, query: "@[2..4]");
            RunTest(expected: new [] { 1, 2, 3 }, input: new [] { 1, 2, 3, 4 }, query: "@[..3]");
            RunTest(expected: new [] { 2, 3, 4 }, input: new [] { 1, 2, 3, 4 }, query: "@[1..]");
            RunTest(expected: new [] { 1, 2 }, input: new [] { 1, 2 }, query: "@[..]");
        }

        [Fact]
        public void NegativeSlices()
        {
            RunTest(expected: new [] { 2 }, input: new [] { 1, 2 }, query: "@[-1..]");
            RunTest(expected: new [] { 1 }, input: new [] { 1, 2 }, query: "@[..-1]");
            RunTest(expected: new [] { 1, 2 }, input: new [] { 0, 1, 2, 3 }, query: "@[-3..-1]");
        }

        [Fact]
        public void ComplexChains()
        {
            RunTest(expected: new [] { 2 }, input: new [] { 0, 1, 2 }, query: "@[1+1..]");
            RunTest(expected: new [] { 0, 1 }, input: new [] { 0, 1, 2 }, query: "@[..1+1]");
        }

        [Fact]
        public void GetDictionaryByKey()
        {
            RunTest(
                expected: "jim", 
                input: ParseObj("{ 'name': 'jim', 'age': 49}"), 
                query: "name");
        }

        [Fact]
        public void KeyAppliedToArray()
        {
            RunTest(
                expected: ParseObj("[1, 2, 3]"),
                query: "[{x: 1}, {x: 2}, {x: 3}].x"
            );

            RunTest(
                expected: ParseObj("[1, 2, 3]"),
                query: "[{x: 1}, {x: 2}, {x: 3}][..].x"
            );
        }

        [Fact]
        public void GetDictionaryByKeyThenIndex()
        {
            RunTest(
                expected: "DeSelms", 
                input: new { name= new [] { "Jim", "DeSelms"}, age = 49}, 
                query: "name[1]");
        }

        [Fact]
        public void GetByIndexThenKey()
        {
            RunTest(
                expected: 49, 
                input: ParseObj("['foo', { 'name': ['Jim', 'DeSelms'], 'age': 49}]"), 
                query: "@[1].age");
        }

        [Fact]
        public void Max()
        {
            RunTest(expected: 50, query: "max([5, 10, 50, 20])");
            RunTest(expected: "C", query: "max(['A', 'B', 'C'])");
        }

        [Fact]
        public void MaxBy()
        {
            RunTest(expected: "success", query: "max_by([{key: 1, value: 'fail'}, {key: 5, value: 'success'}, {key: 2, value: 'fail'}], &key).value");
            RunTest(expected: "success", query: "max_by([{key: 1, value: 'success'}, {key: 5, value: 'foo'}, {key: 2, value: 'fail'}], &(-key)).value");
        }

        [Fact]
        public void MinBy()
        {
            RunTest(expected: "yay", query: "min_by([{key: 1, value: 'yay'}, {key: 5, value: 'success'}, {key: 2, value: 'fail'}], &key).value");
            RunTest(expected: "hooray", query: "min_by([{key: 1, value: 'foo'}, {key: 5, value: 'hooray'}, {key: 2, value: 'fail'}], &(-key)).value");
        }


        [Fact]
        public void Min()
        {
            RunTest(expected: 10, query: "min([10, 20])");
            RunTest(expected: "A", query: "min(['A', 'B', 'C'])");
        }

        [Fact]
        public void QuotedIdentifier()
        {
            RunTest(
                expected: 49, 
                input: ParseObj("['foo', { 'name': ['Jim', 'DeSelms'], 'the age': 49}]"), 
                query: "@[1]['the age']");
        }

        [Fact]
        public void QuotedIdentifierWithDoubleQuotes()
        {
            RunTest(
                expected: 49, 
                input: ParseObj("['foo', { 'name': ['Jim', 'DeSelms'], 'the age': 49}]"), 
                query: "@[1][\"the age\"]");
        }

        [Fact]
        public void GetArrayIdentityThenKey()
        {
            RunTest(
                expected: new [] { 50, 49 }, 
                input: new [] { 
                    new { name = new [] { "Fred", "Dingles" }, age = 50 },
                    new { name = new [] { "Jim", "DeSelms"}, age = 49 }
                },
                query: "@[..].age");
        }

        [Fact]
        public void ChainOfKeys()
        {
            RunTest(
                expected: 123, 
                input: ParseObj("{'a': { 'b': { 'c': { 'd': 123 }}}}"), 
                query: "@.a.b.c.d");
        }

        [Fact]
        public void KeyThenIndex()
        {
            RunTest(
                expected: 123, 
                input: new[] {123, 234}, 
                query: "@[@ == 123][0]");
        }

        [Fact]
        public void ChainOfIndexes()
        {
            RunTest(
                expected: 5, 
                input: ParseObj("[1, [2, [3, [4, 5]]]]"), 
                query: "@[1][1][1][1]");
        }

        [Fact]
        public void ToNumber()
        {
            RunTest(3.14, query: "to_number('3.14')");
        }

        [Fact]
        public void ToMap()
        {
            RunTest(expected: ParseObj("{afoo: 'ax', bfoo: 'bx'}"), query: "to_map(['a', 'b'], &(@ + 'foo'), &(@ + 'x'))");
            RunTest(expected: "<error: (1, 13) argument arg2: expected IBifoqlExpression, got AsyncString instead.>", query: "to_map([1], 'a', 'b')");
        }

        [Fact]
        public void Reverse()
        {
            RunTest(
                expected: new object[] { 5, 4, 3, 2, 1 }, 
                input: new object[] { 1, 2, 3, 4, 5 }, 
                query: "reverse(@)");
        }

        [Fact]
        public void SingleIndex()
        {
            RunTest(
                expected: 2, 
                input: ParseObj("[1, 2]"), 
                query: "@[1]");
        }

        [Fact]
        public void Sorting()
        {
            RunTest(
                expected: new [] { "1", "2", "3" }, 
                input: new [] { "2", "3", "1" }, 
                query: "sort(@)");
        }

        [Fact]
        public void SortingAndSpecifyingKey()
        {
            RunTest(
                expected: new [] { new {a="1"}, new {a="2"}, new{a="3"} }, 
                input: new [] { new {a="2"}, new {a="3"}, new{a="1"} }, 
                query: "sort_by(@, &a)");
        }

        [Fact]
        public void SortingAndSpecifyingKey2()
        {
            RunTest(
                expected: new [] { new {a="1"}, new {a="2"}, new{a="3"} }, 
                input: new [] { new {a="2"}, new {a="3"}, new{a="1"} }, 
                query: "sort_by(@, &(a))");
        }

        [Fact]
        public void SortingWithPipe()
        {
            RunTest(
                expected: new [] { "X", "Z", "Y" }, 
                input: new [] { 
                    new { a = new { b = "1", c = "X" } },
                    new { a = new { b = "3", c = "Y" } },
                    new { a = new { b = "2", c = "Z" } },
                 }, 
                query: "(@[..].a) | sort_by(@, &b) | @[..].c");
        }

        [Fact]
        public void SortingByNumber()
        {
            RunTest(
                expected: new [] { "b", "c", "a" }, 
                input: new [] { 
                    new { a = new { b = 5, c = "a" } },
                    new { a = new { b = 3, c = "b" } },
                    new { a = new { b = 4, c = "c" } },
                 }, 
                query: "(@[..].a) | sort_by(@, &b) | @[..].c");
        }

        [Fact]
        public void PipeArrayToMany()
        {
            RunTest(
                expected: new [] { 2, 4, 6, 8 },
                input: new [] { 1, 2, 3, 4 },
                query: "@ |< @*2"
            );
        }

        [Fact]
        public void MapProjection()
        {
            RunTest(
                expected: new { greeting = "hello", name = "world" }, 
                input: new [] { "hello", "world" },
                query: "{greeting: @[0], name: @[1]}");
        }

        [Fact]
        public void NumberExpr()
        {
            RunTest(
                expected: 3.14159d,
                input: new object(),
                query: "3.14159"
            );
        }

        [Fact]
        public void StringExpr()
        {
            RunTest(
                expected: "hello",
                input: new object(),
                query: "`hello`"
            );
        }

        [Fact]
        public void StringExpr2()
        {
            RunTest(
                expected: "hello",
                input: new object(),
                query: "'hello'"
            );
        }

        [Fact]
        public void IntExpr()
        {
            RunTest(
                expected: 1.2,
                input: new object(),
                query: "1.2"
            );
        }

        [Fact]
        public void SortPipedToKey()
        {
            RunTest(
                expected: new [] { "A", "B" },
                input: new [] { 
                    new { content = new { name="B" } }, 
                    new { content = new { name="A" } }, 
                    },
                query: "sort_by(@, &content.name) | @[..].content.name"
            );
        }

        [Fact]
        public void Negative()
        {
            RunTest(
                expected: -5,
                query: "-5"
            );
        }

        [Fact]
        public void Abs()
        {
            RunTest(
                expected: 3.5,
                query: "abs(-3.5)"
            );
        }

        [Fact]
        public void Avg()
        {
            RunTest(
                expected: 3,
                query: "avg([ 2 , 3 , 4 ])"
            );
        }

        [Fact]
        public void Boolean()
        {
            RunTest(
                expected: true,
                query: "true"
            );
            RunTest(
                expected: false,
                query: "false"
            );
        }

        [Fact]
        public void ContainsString()
        {
            RunTest(
                expected: true,
                query: "contains(`abc`, `bc`)"
            );
            RunTest(
                expected: false,
                query: "contains(`abc`, `l`)"
            );
        }

        [Fact]
        public void ContainsArray()
        {
            RunTest(
                expected: true,
                query: "contains([1,2,3], 1)"
            );
            RunTest(
                expected: false,
                query: "contains([1,2,3], 5)"
            );
        }

        [Fact]
        public void EqualTo()
        {
            RunTest(expected: true, query: "1 == 1");
            RunTest(expected: false, query: "1 == 2");
            RunTest(expected: false, query: "true == false");
            RunTest(expected: true, query: "false == false");
            RunTest(expected: true, query: "null == null");

            RunTest(expected: false, query: "1 != 1");
            RunTest(expected: true, query: "1 != 2");
            RunTest(expected: true, query: "'Jim' != 'Fred'");
        }

        [Fact]
        public void EqualToArrays()
        {
            RunTest(expected: true, query: "[] == []");
            RunTest(expected: false, query: "[] == [1]");
            RunTest(expected: false, query: "[1] == []");
            RunTest(expected: true, query: "[1] == [1]");
            RunTest(expected: false, query: "[1] == ['1']");

            RunTest(expected: false, query: "[] != []");
            RunTest(expected: true, query: "[] != [1]");
            RunTest(expected: true, query: "[1] != []");
            RunTest(expected: false, query: "[1] != [1]");
            RunTest(expected: true, query: "[1] != ['1']");
        }

        [Fact]
        public void EqualToDicts()
        {
            RunTest(expected: true, query: "{} == {}");
            RunTest(expected: true, query: "{x: 1} == {x: 1}");
            RunTest(expected: false, query: "{x: 1} == {x: 1, y: 2}");
            RunTest(expected: false, query: "{x: 1, foo: 'bar'} == {x: 1, y: 2}");
            RunTest(expected: false, query: "{x: 1} == {x: 2}");
            RunTest(expected: true, query: "{x: 1, y: 2} == {y: 5-3, x: 1}");

            RunTest(expected: false, query: "{} != {}");
            RunTest(expected: false, query: "{x: 1} != {x: 1}");
            RunTest(expected: true, query: "{x: 1} != {x: 1, y: 2}");
            RunTest(expected: true, query: "{x: 1} != {x: 2}");
            RunTest(expected: false, query: "{x: 1, y: 2} != {y: 5-3, x: 1}");
        }

        [Fact]
        public void AndOr()
        {
            RunTest(expected: true, query: "1 == 2 || 3 == 3");
            RunTest(expected: false, query: "1 == 2 || false");
            RunTest(expected: false, query: "true && false");
            RunTest(expected: true, query: "true && true");
            RunTest(expected: true, query: "false && false || true");
        }

        [Fact]
        public void Arithmetic()
        {
            RunTest(expected: 5, query: "2 + 3");
            RunTest(expected: -9.5, query: "10 - 19.5");
            RunTest(expected: 77, query: "11 * 7");
            RunTest(expected: -5, query: "10 / -2");
            RunTest(expected: 2, query: "8 % 3");

            RunTest(expected: 8, query: "2 + 3 * 4 - 12 / 2");
        }

        [Fact]
        public void DivByZero()
        {
            RunTest(expected: "<error: (1, 1) division by zero>", query: "5 / 0");
        }

        [Fact]
        public void StringAndArrayConcatenation()
        {
            RunTest(expected: "Hello world", query: "'Hello' + ' world'");
            RunTest(expected: new [] { "a", "b", "c", "d"}, query: "['a', 'b'] + ['c', 'd']");
            RunTest(expected: new [] { "a", "b", "c" }, query: "['a', 'b'] + 'c'");
        }

        [Fact]
        public void StringAndArraySubtraction()
        {
            RunTest(expected: "B F", query: "'Big Fig' - 'ig'");
            RunTest(expected: new [] { 2 }, query: "[1, 2, 3] - [1, 3]");
            RunTest(expected: new [] { 2 }, query: "[1, 2, 1] - 1");
        }

        [Fact]
        public void Ternary()
        {
            RunTest(expected: 5, query: "true ? 5 : 100");
            RunTest(expected: 20, query: "false ? 5 : 20");
            RunTest(expected: 10, query: "1 == 2 ? 3 : 4 == 5 ? 6 : 7 == 8 ? 9 : 10");
        }

        [Fact]
        public void Variables()
        {
            RunTest(expected: 6, query: "$x = 1; $y = 2; $z = 3; $x + $y + $z");
        }

        [Fact]
        public void RootObjectVariable()
        {
            RunTest(expected: 5, input: 5, query: "$");
        }

        [Fact]
        public void Inequalities()
        {
            RunTest(expected: true, query: "1 < 2");
            RunTest(expected: false, query: "1 < 1");
            RunTest(expected: true, query: "1 <= 2");
            RunTest(expected: true, query: "1 <= 1");
            RunTest(expected: false, query: "1 <= 0");

            RunTest(expected: true, query: "2 > 1");
            RunTest(expected: false, query: "2 > 2");
            RunTest(expected: true, query: "2 >= 1");
            RunTest(expected: true, query: "2 >= 2");
            RunTest(expected: false, query: "1 >= 2");
        }

        [Fact]
        public void StartsWith()
        {
            RunTest(expected: true, query: "starts_with('hello', 'he')");
            RunTest(expected: true, query: "starts_with('hello', '')");
            RunTest(expected: false, query: "starts_with('hello', 'e')");
        }

        [Fact]
        public void EndsWith()
        {
            RunTest(expected: true, query: "ends_with('hello', 'lo')");
            RunTest(expected: true, query: "ends_with('hello', '')");
            RunTest(expected: false, query: "ends_with('hello', 'h')");
        }

        [Fact]
        public void Filter()
        {
            RunTest(expected: new [] { 1, 2 }, input: new [] { 1, 2, 3 }, query: "@[@ < 3]");
            RunTest(expected: new object[0], input: new [] { "Hello" }, query: "@[false]");
            RunTest(expected: new object[] { "Hello" }, input: new [] { "Hello" }, query: "@[true]");
        }

        [Fact]
        public void Filter2()
        {
            var obj = new Dictionary<string, object> { ["name"] = "Frank", ["length"] = 5 };
            RunTest(expected: 5, input: new [] { obj }, query: "@[0].length");
            RunTest(expected: new [] { 5 }, input: new [] { obj }, query: "@[true].length");
        }

        [Fact]
        public void VariableSpread()
        {
            RunTest(
                expected: ParseObj("{a: 1, b: 2}"),
                query: "$x = {a: 1}; {...$x, b: 2}"
            );
        }

        [Fact]
        public void VariablePrecedence()
        {
            RunTest(
                expected: ParseObj("[1]"),
                query: "$x = [1,2][0]; [$x]"
            );
        }

        [Fact]
        public void VariableSpreadOverride()
        {
            RunTest(
                expected: ParseObj("{a: 2}"),
                query: "$x = {a: 1}; $y = {a: 2}; {...$x, ...$y}"
            );
        }

        [Fact]
        public void ArraySpread()
        {
            RunTest(
                expected: ParseObj("[1,2,3,4,5]"),
                query: "[1, ...[2,3], 4, 5]"
            );
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
        public void EvalTestWithExprInVariable()
        {
            RunTest(
                expected: 50,
                query: "$five = 5; $timesTen = &(@ * 10); $five | eval($timesTen)"
            );
        }

        [Fact]
        public void EvalTest()
        {
            RunTest(
                expected: 10,
                query: "5 | eval(&(@ * 2))"
            );
        }

        [Fact]
        public void ErrorFunctionNotString()
        {
            // TODO: We want to serialize the object here, but I don't want to depend on Newtonsoft.
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
        public void IndexOutOfBoundsReturnsNull()
        {
            RunTest(expected: null, query: "[][0]");
        }

        [Fact]
        public void NullCoalesceWhenNull()
        {
            RunTest(expected: "hello", query: "[][0] ?? 'hello'");
        }

        [Fact]
        public void NullCoalesceWhenNotNull()
        {
            RunTest(expected: 5, query: "[5][0] ?? 6");
        }

        [Fact]
        public void NullCoalesceWhenError()
        {
            RunTest(expected: 6, query: "{a: 1}.b ?? 6");
        }

        [Fact]
        public void Indexes()
        {
            RunTest(expected: 1, query: "[1, 2, 3][0]");
            RunTest(expected: 2, query: "[1, 2, 3][1]");
            RunTest(expected: 3, query: "[1, 2, 3][2]");
            RunTest(expected: null, query: "[1, 2, 3][3]");
            RunTest(expected: 2, query: "[1, 2, 3][-2]");
            RunTest(expected: 1, query: "[1, 2, 3][-3]");
            RunTest(expected: null, query: "[1, 2, 3][-4]");
        }

        [Fact]
        public void PassedVariable()
        {
            RunTest(expected: 123, query: "$num", arguments: new Dictionary<string, object> { ["num"] = 123 });
        }

        [Fact]
        public void Map()
        {
            RunTest(expected: ParseObj("{\"x\": 3, \"y\": 4}"), input: 2, query: "$v = 2; {x: 1+$v, y: 2+$v}");
        }

        [Fact]
        public void Distinct()
        {
            RunTest(expected: ParseObj("[1, 2, 3]"), query: "distinct([1, 1, 2, 3, 1, 2, 2, 1, 3])");
        }

        [Fact]
        public void Flatten()
        {
            RunTest(expected: ParseObj("[1, 2, 3, [4, 5], 6]"), query: "flatten([1, [2, 3], [[4, 5]], 6])");
        }
        
        [Fact]
        public void IfError()
        {
            RunTest(expected: 5, query: "if_error(error('x'), 5)");
        }

        [Fact]
        public void UndefinedObjectIsOmittedFromArrayProjection()
        {
            RunTest(expected: new object[] { 5 }, query: "[ 5, [][0] ]");
            RunTest(expected: new object[] { 5 }, query: "[ 5, {}.x ]");
            RunTest(expected: new object[] { 5 }, query: "[ 5, {}['x'] ]");
        }

        [Fact]
        public void UndefinedObjectIsOmittedFromMapProjection()
        {
            RunTest(expected: new { x=5 }, query: "{x: 5, y: [][1] }");
            RunTest(expected: new { x=5 }, query: "{x: 5, y: {}.foo }");
            RunTest(expected: new { x=5 }, query: "{x: 5, y: {}['foo'] }");
        }

        [Fact]
        public void UndefinedObjectResolvesToNull()
        {
            RunTest(expected: null, query: "[][0]");
        }

        [Fact]
        public void CustomFunctionTest()
        {
            Func<IBifoqlNumber, Task<IBifoqlObject>> timesTwo = async (IBifoqlNumber n) => (await n.Value * 2).ToBifoqlObject();
            var functions = new Dictionary<string, CustomFunction>
            {
                ["timestwo"] = CustomFunction.Create<IBifoqlNumber>(timesTwo)
            };
            RunTest(expected: 16, query: "timestwo(8)", customFunctions: functions);
        }

        [Fact]
        public void MapProjectionShorthand()
        {
            RunTest(
                expected: new { obj = new { age = 20 }},
                query: "{ obj: { name: 'Fred', age: 20, shoeSize: 10 } } | { obj { age } }"
            );
            RunTest(
                expected: new { obj = new { age = 20 }},
                query: "{ obj: { name: 'Fred', age: 20, shoeSize: 10 } } | { obj | { age } }"
            );
        }

        [Fact]
        public void MapProjectionArrayShorthand()
        {
            RunTest(
                expected: new { obj = new [] { new { age = 20 }, new { age = 30 }}},
                query: @"{ obj: [{ name: 'Fred', age: 20, shoeSize: 10 }, { name: 'Steve', age: 30, shoeSize: 11 }] } 
                    | { obj |< { age } }"
            );
        }

        private static void RunTest(object expected, string query, object input=null, IReadOnlyDictionary<string, object> arguments=null, IReadOnlyDictionary<string, CustomFunction> customFunctions=null)
        {
            var inputJson = JsonConvert.SerializeObject(input);
            var jobject = JsonConvert.DeserializeObject<object>(inputJson);
            var originalJson = JsonConvert.SerializeObject(jobject);
            var asyncObj = ObjectConverter.ToAsyncObject(jobject);

            var result = Query(asyncObj, query, arguments, customFunctions).Result;

            var actualJson = JsonConvert.SerializeObject(result);

            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        private static object ParseObj(string json)
        {
            return JsonConvert.DeserializeObject<object>(json);
        }

        private static async Task<object> Query(object o, string query, IReadOnlyDictionary<string, object> arguments, IReadOnlyDictionary<string, CustomFunction> customFunctions)
        {
            var queryObj = Bifoql.Query.Compile(query, customFunctions);
            var asyncObj = o.ToBifoqlObject();

            return await queryObj.Run(asyncObj, arguments);
        }
    }
}
