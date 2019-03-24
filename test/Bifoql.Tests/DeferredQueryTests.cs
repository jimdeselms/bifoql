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
    public class DeferredQueryTests
    {
        [Fact]
        public void JustGetFullObject()
        {
            RunTest(5, 5, "remote");
            RunTest(new [] { 1 }, new [] { 1 }, "remote");
        }

        [Fact]
        public void DeferredQueryWithIndex()
        {
            RunTest(1, new [] { 1 }, "remote[0]");
        }

        [Fact]
        public void DeferQueryFilter()
        {
            RunTest(new [] { 2 }, new [] { 1, 2 }, "remote[? @ > 1]");
        }

        [Fact]
        public void DeferKey()
        {
            RunTest("Jimmy", new { name = "Jim" }, "remote.name + 'my'");
        }

        [Fact]
        public void DeferFunction()
        {
            RunTest(50, 5.923d, "remote | floor(@) | @ * 10");
        }

        [Fact]
        public void DeferArrayPipe()
        {
            RunTest(new [] { 10, 20 }, new [] { 1, 2 }, "remote |< @ * 10");
        }

        
        private void RunTest(object expected, object remoteObject, string query)
        {
            var testObj = new
            {
                remote = new DeferredQueryObject(remoteObject),
            }.ToBifoqlObject();

            var compiledQuery = Query.Compile(query);
            var result = compiledQuery.Run(testObj).Result;

            var actualJson = JsonConvert.SerializeObject(result);
            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        private static IBifoqlObject TEST_OBJ = new[] {
                    new { name = "Fred", age = 35 },
                    new { name = "Wilma", age = 30 },
                    new { name = "Pebbles", age = 3 }
                }.ToBifoqlObject();

        private class DeferredQueryObject : IBifoqlDeferredQueryInternal
        {
            private IBifoqlObject _remoteObject;

            public DeferredQueryObject(object remoteObject)
            {
                _remoteObject = remoteObject.ToBifoqlObject();
            }

            public async Task<object> EvaluateQuery(string query)
            {
                // We'll simulate a service call where we get a query and pass it along to a service where
                // it gets compiled.

                // If this were a real remote service, the compilation would happen on the other side.
                var compiledQuery = Query.Compile(query);
                return (await compiledQuery.Run(_remoteObject));
            }

            public Task<bool> IsEqualTo(IBifoqlObject o)
            {
                return Task.FromResult(o is DeferredQueryObject);
            }

        }
    }
}
