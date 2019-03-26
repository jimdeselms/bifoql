using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using Bifoql.Types;

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
    }
}
