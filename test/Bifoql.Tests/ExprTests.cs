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
    public class ExprTests : ExprTestBase
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
        public void SimpleKey()
        {
            RunTest(
                expected: 1,
                query: "{a:1}.a"
            );
        }

        [Fact]
        public void KeyNotFound()
        {
            RunTest(
                expected: "<error: (1, 7) key 'b' not found>",
                query: "{a:1}.b"
            );
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
            RunTest(expected: new [] { 1, 2 }, input: new [] { 0, 1, 2, 3 }, query: "@[1..-1]");
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
                query: "@[? @ == 123][0]");
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
                expected: new [] { "1", "2", "3" }, 
                input: new [] { new {a="2"}, new {a="3"}, new{a="1"} }, 
                query: "sort_by(@, &a).a");
        }

        [Fact]
        public void SortingAndSpecifyingKey2()
        {
            RunTest(
                expected: new [] { "1", "2", "3" }, 
                input: new [] { new {a="2"}, new {a="3"}, new{a="1"} }, 
                query: "sort_by(@, &(a)).a");
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
            RunTest(
                expected: -5,
                query: "$i = 5; -$i"
            );
        }

        [Fact]
        public void Not()
        {
            RunTest(
                expected: true,
                query: "!false"
            );
            RunTest(
                expected: false,
                query: "!true"
            );
            RunTest(
                expected: true,
                query: "!null"
            );
            RunTest(
                expected: true,
                query: "!undefined"
            );
            RunTest(
                expected: false,
                query: "!'not boolean'"
            );
            RunTest(
                expected: true,
                query: "!''"
            );
            RunTest(
                expected: false,
                query: "![1]"
            );
            RunTest(
                expected: true,
                query: "![]"
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
        public void VariableCaseSensitivity()
        {
            var args = new Dictionary<string, object>() { ["x"] = 2};
            RunTest(expected: 2, query: "$x", arguments: args);
            RunTest(expected: "<error: (1, 1) reference to undefined variable '$X'>", query: "$X", arguments: args);

            var argsUppercase = new Dictionary<string, object>() { ["X"] = 2};
            RunTest(expected: "<error: (1, 1) reference to undefined variable '$x'>", query: "$x", arguments: argsUppercase);
            RunTest(expected: 2, query: "$X", arguments: argsUppercase);
        }
        

        [Fact]
        public void VariableScopingWithArray()
        {
            // At one point I had variable scoping.
            // I'm taking it out. It's too much.
            RunTest(
                expected: "<error: (1, 14) expected )>", 
                query: "([1,2] |< ($x=@; $x*10)) | sum(@)");
        }

        [Fact]
        public void VariableScoping()
        {
            RunTest(
                expected: "<error: (1, 8) expected )>", 
                query: "1 | ($x=@; $x)");
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
            RunTest(expected: new [] { 1, 2 }, input: new [] { 1, 2, 3 }, query: "@[?@ < 3]");
            RunTest(expected: new object[0], input: new [] { "Hello" }, query: "@[?false]");
            RunTest(expected: new object[] { "Hello" }, input: new [] { "Hello" }, query: "@[?true]");
        }

        [Fact]
        public void Filter2()
        {
            var obj = new Dictionary<string, object> { ["name"] = "Frank", ["length"] = 5 };
            RunTest(expected: 5, input: new [] { obj }, query: "@[0].length");
            RunTest(expected: new [] { 5 }, input: new [] { obj }, query: "@[?true].length");
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
        public void EvalTestWithExprInVariable()
        {
            RunTest(
                expected: 50,
                query: "$five = 5; $timesTen = &(@ * 10); $five | *$timesTen"
            );
        }

        [Fact]
        public void EvalTest()
        {
            RunTest(
                expected: 10,
                query: "5 | *&(@ * 2)"
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
        public void Indexes()
        {
            RunTest(expected: 1, query: "[1, 2, 3][0]");
            RunTest(expected: 2, query: "[1, 2, 3][1]");
            RunTest(expected: 3, query: "[1, 2, 3][2]");
            RunTest(expected: null, query: "[1, 2, 3][3]");
        }

        [Fact]
        public void NegativeIndexes()
        {
            RunTest(expected: 2, query: "[1, 2, 3][-2]");
            RunTest(expected: 1, query: "[1, 2, 3][-3]");
            RunTest(expected: null, query: "[1, 2, 3][-4]");
        }

        [Fact]
        public void PassedVariable()
        {
            RunTest(
                expected: 123, 
                query: "$num", 
                arguments: new Dictionary<string, object> { ["num"] = 123 });
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
            RunTest(expected: 5, query: "error('x') ?? 5");
        }

        [Fact]
        public void UndefinedObjectIsOmittedFromArrayProjection()
        {
            RunTest(expected: new object[] { 5 }, query: "[ 5, [][0] ]");
            RunTest(expected: new object[] { 5 }, query: "[ 5, undefined ]");
        }

        [Fact]
        public void UndefinedObjectIsOmittedFromMapProjection()
        {
            RunTest(expected: new { x=5 }, query: "{x: 5, y: [][1] }");
            RunTest(expected: new { x=5 }, query: "{x: 5, y: undefined }");
        }

        [Fact]
        public void UndefinedObjectResolvesToNull()
        {
            RunTest(expected: null, query: "[][0]");
        }

        [Fact]
        public void Undefined()
        {
            // These demonstrate that undefined resolves to null,
            // but if it's an array or map entry, then it causes the entry to be omitted.
            RunTest(expected: null, query: "undefined");
            RunTest(expected: new object[0], query: "[undefined]");
            RunTest(expected: new [] { 5 }, query: "[undefined, 5]");
            RunTest(expected: new {}, query: "{foo: undefined}");
            RunTest(expected: new {x = 123}, query: "{foo: undefined, x: 123}");

            // This shows that you can convert null into undefined to cause an element to
            // be omitted from a list
            RunTest(expected: new object[0], query: "[null ?? undefined]");
        }

        [Fact]
        public void CustomFunctionOneArgTest()
        {
            Func<int, object> timesTwo = x => x * 2;
            var functions = new Dictionary<string, CustomFunction>
            {
                ["timestwo"] = CustomFunction.Create<int>(timesTwo)
            };
            RunTest(expected: 16, query: "timestwo(8)", customFunctions: functions);
        }

        [Fact]
        public void CustomFunctionTwoArgTest()
        {
            Func<string, string, object> split = (s1, c) => s1.Split(c[0]);

            var functions = new Dictionary<string, CustomFunction>
            {
                ["split"] = CustomFunction.Create(split)
            };

            RunTest(expected: new [] { "ab", "cd" }, query: "split('ab|cd', '|')", customFunctions: functions);
        }

        [Fact]
        public void CustomFunctionThreeArgTest()
        {
            Func<int, int, int, object> add3 = (i1, i2, i3) => i1 + i2 + i3;

            var functions = new Dictionary<string, CustomFunction>
            {
                ["add3"] = CustomFunction.Create(add3)
            };

            RunTest(expected: 6, query: "add3(1, 2, 3)", customFunctions: functions);
        }
    }
}
