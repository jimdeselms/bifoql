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
using Bifoql.Types;
using Bifoql.Adapters;

namespace Bifoql.Tests
{
    public class MapTests : ExprTestBase
    {
        // These are tests for various syntaxes that are allowed for map projection.
        [Fact]
        public void MapProjectionRootLevel()
        {
            RunTest(
                expected: new { greeting = "hello", name = "world" }, 
                input: new [] { "hello", "world" },
                query: "{greeting: @[0], name: @[1]}");

            RunTest(
                expected: new { greeting = "hello", name = "world" }, 
                input: new [] { new { g="hello", n="pardner" }, new { g="howdy", n="world" }},
                query: "{greeting: @[0].g, name: @[1].n}");
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

        [Fact]
        public void MapProjectionFollowingMap()
        {
            RunTest(
                expected: new { x = 1, z = 8 },
                query: "{x:1, y:2, z:3} { x, z: z+5 }"
            );
        }

        [Fact]
        public void MapProjectionFollowingArray()
        {
            RunTest(
                expected: new [] { new { x = 1 }, new { x = 2 }},
                query: "[{x:1, y:2}, {x:2, y:3}] { x }"
            );
        }

        [Fact]
        public void NestedSimplifiedSyntax()
        {
            RunTest(
                expected: ParseObj("{a: {b: [{c: 1}, {c: 2}], greeting: 'hello'}}"),
                query: "{a: {b: [{c: 1}, {c: 2}], greeting: 'hello'}} { a { b { c }, greeting } }"
            );
        }

        [Fact]
        public void SimplifiedSyntax()
        {
            RunTest(
                expected: ParseObj("{a: 1}"),
                query: "{a: 1} {a}"
            );

            RunTest(
                expected: new { a = 1 },
                query: "$x = {a: 1}; $x {a}"
            );

            RunTest(
                expected: new [] { new { a = 1 } },
                query: "$x = {a: 1}; [$x] {a}"
            );

            RunTest(
                expected: ParseObj("[{a: 1}]"),
                query: "$x = [{a: 1}]; $x {a}"
            );
        }

        [Fact]
        public void SimplifiedSyntaxObjectComesFromContext()
        {
            RunTest(
                input: new { a = new { b = new { c = new { d = new { e = 5}}}}},
                expected: new { a = new { b = new { c = new { d = new { e = 5}}}}},
                query: "{a {b {c {d { e } } } } }"
            );

            RunTest(
                input: new { a = new { b = new [] {new { c = new { d = new [] { new { e = 5}, new { e = 5}}}}}}},
                expected: new { a = new { b = new [] {new { c = new { d = new [] { new { e = 5}, new { e = 5}}}}}}},
                query: "{a {b {c {d { e } } } } }"
            );
        }

        [Fact]
        public void NestedSimplifiedSyntaxWithRenames()
        {
            RunTest(
                expected: ParseObj("{newa: {newb: [{newc: 1}, {newc: 2}], newgreeting: 'hello'}}"),
                query: "{a: {b: [{c: 1}, {c: 2}], greeting: 'hello'}} { newa: a { newb: b { newc: c }, newgreeting: greeting } }"
            );
        }

        [Fact]
        public void NestedSimplifiedSyntaxWithLookups()
        {
            // Similar to the test above, but with lookups.
            var lookup = new MockSyncLookup("a",
                new MockSyncLookup("b", new [] {
                    new MockSyncLookup("c", 1),
                    new MockSyncLookup("c", 2)
                }).ToBifoqlObject());

            var map = new { a = new {b = new [] {new { c = 1 }, new { c = 2 }}}};

            RunTest(
                expected: ParseObj("{a: {b: [{c: 1}, {c: 2}]}}"),
                input: lookup,
                query: "@ | { a { b { c } } }"
            );
        }

        [Fact]
        public void NestedSimplifiedSyntaxWithLookupsAsync()
        {
            // Similar to the test above, but with lookups.
            var lookup = new MockAsyncLookup("a",
                new MockAsyncLookup("b", new [] {
                    new MockAsyncLookup("c", 1),
                    new MockAsyncLookup("c", 2)
                }).ToBifoqlObject());

            var map = new { a = new {b = new [] {new { c = 1 }, new { c = 2 }}}};

            RunTest(
                expected: ParseObj("{a: {b: [{c: 1}, {c: 2}]}}"),
                input: lookup,
                query: "@ | { a { b { c } } }"
            );
        }

        [Fact]
        public void NestedSimplifiedSyntaxWithLookupsTrivial()
        {
            // Similar to the test above, but with lookups.
            var lookup = new MockSyncLookup("a", 1);

            RunTest(
                expected: ParseObj("{a: 1}"),
                input: lookup,
                query: "{a}"
            );
        }

        [Fact]
        public void ToMap()
        {
            RunTest(expected: ParseObj("{afoo: 'ax', bfoo: 'bx'}"), query: "to_map(['a', 'b'], &(@ + 'foo'), &(@ + 'x'))");
            RunTest(expected: "<error: (1, 13) argument arg2: expected IBifoqlExpression, got AsyncString instead.>", query: "to_map([1], 'a', 'b')");
        }
    }
}
