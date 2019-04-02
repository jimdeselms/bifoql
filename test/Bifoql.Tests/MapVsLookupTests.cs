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

namespace Bifoql.Tests
{
    public class MapVsLookupTests : ExprTestBase
    {
        // Maps and lookups have a big distinction:
        // Maps are full-fledged dictionaries that you can enumerate on and get the keys on, etc.
        // Lookups only allow you to look things up.
        //
        // The distinction is important; if a query resolves to a map, then the entire map will be returned. If a query resolves to a lookup, it will fail.
        // The intent here is that a map gives you an easy way to get a relatively large object, while a lookup forces you to explicitly request individual elements.
        // A lookup is how GraphQL works; in GraphQL, you can't request a non-leaf object; you must explicitly request each field.
        //
        // Bifoql is less strict; if the schema designer wants, they can expose maps for some objects, creating very concise queries. But some objects might produce very
        // large results, or cycles. For those, return a lookup.
        //
        // An easy example is a map with a "parent" element. If you naively expose a parent, then the child -- the thing you initially requested -- will be returned in the
        // result, and this will cause a cycle. If the "parent" is part of a lookup, then the elements of the lookup must be explicitly requested.

        [Fact]
        public void LookupWithMap()
        {
            var fred = new Person { Name = "Fred", Address = new Address("1 Main Street", "12345" ) };
            var george = new Person { Name = "George", Address = new Address("2 Maple Street", "23456" ) };
            var martha = new Person { Name = "Martha", Address = new Address("2 Maple Street", "23456" ) };

            fred.Mother = martha;
            fred.Father = george;

            var obj = fred.ToBifoqlObject();

            RunTest(
                expected: "12345",
                input: obj,
                query: "Address.ZipCode");

            RunTest(
                expected: new { Name = "George", Address = new { Street = "2 Maple Street", ZipCode = "23456" } },
                input: obj,
                query: "Father | { Name, Address }");
        }

        [Fact]
        public void MapThatContainsLookup()
        {
            var george = new Person { Name = "George", Address = new Address("2 Maple Street", "23456" ) };
            var martha = new Person { Name = "Martha", Address = new Address("2 Maple Street", "23456" ) };

            var parents = new {
                Father = george,
                Mother = martha,
                Name = "Steve"
            };

            RunTest(
                expected: "George",
                input: parents.ToBifoqlObject(),
                query: "Father.Name");

            // Since we're not requesting any fields, we'll get an error back.
            RunTest(
                expected: "<error: query must resolve to leaf nodes>",
                input: parents.ToBifoqlObject(),
                query: "@");
        }

        [Fact]
        public void ArrayWithLookup()
        {
            var address = new Address("2 Maple Street", "23456");
            var george = new Person { Name = "George", Address = address };

            var obj = (new object[] { address, george }).ToBifoqlObject();

            // We should only get the address, since george is a lookup.
            RunTest(
                expected: new object[] { new { Street = "2 Maple Street", ZipCode = "23456" }, null},
                input: obj,
                query: "@");

            RunTest(
                expected: "23456",
                input: obj,
                query: "@[0].ZipCode"
            );
        }

        [Fact]
        public void MapWithArrayOfMaps()
        {
            var obj = new MockSyncLookup("arr", new MockSyncArray(new MockSyncLookup("v", "this/foo")));

            RunTest(
                input: obj,
                query: @"$x = { arr: [ { v: 'this/foo' }, { v: 'this/bar' } ] }; $x.arr { v }",
                expected: new [] { new { v = "this/foo"}, new { v = "this/bar"} });
        }


        internal class Person : IBifoqlLookupSync
        {
            public object Mother { get; set; }
            public object Father { get; set; }
            public string Name { get; set; }

            public Address Address { get; set; }

            public bool TryGetValue(string key, out Func<object> result)
            {
                var success = true;
                switch(key)
                {
                    case "Mother": result = () => Mother; break;
                    case "Father": result = () => Father; break;
                    case "Name": result = () => Name; break;
                    case "Address": result = () => Address; break;
                    default:
                        success = false;
                        result = null;
                        break;
                }
                return success;
            }
        }

        internal class Address : IBifoqlMapSync
        {
            public string _street { get; set; }
            public string _zipCode { get; set; }

            public Address(string street, string zipCode)
            {
                _street = street;
                _zipCode = zipCode;
            }

            public IReadOnlyDictionary<string, Func<object>> Items => new Dictionary<string, Func<object>>
            {
                ["Street"] = () => _street,
                ["ZipCode"] = () => _zipCode
            };
        }
    }

}
