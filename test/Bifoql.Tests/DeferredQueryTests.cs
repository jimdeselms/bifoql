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
        public void DeferQueryFilter()
        {
            RunTest(1, new [] { 1 }, "remote[0]");
            RunTest(new [] { 2 }, new [] { 1, 2 }, "remote[@ > 1]");
        }

        [Fact]
        public void DeferKey()
        {
            RunTest("Jimmy", new { name = "Jim" }, "remote.name + 'my'");
        }

        [Fact]
        public void DeferFunction()
        {
            RunTest(5, 5.923d, "remote | floor(@)");
        }

        
        private void RunTest(object expected, object remoteObject, string query)
        {
            var testObj = new
            {
                remote = new DeferredQueryObject(remoteObject),
            }.ToAsyncObject();

            var compiledQuery = Query.Compile(query);
            var result = compiledQuery.Run(testObj).Result;

            var actualJson = JsonConvert.SerializeObject(result);
            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        private static IAsyncObject TEST_OBJ = new[] {
                    new { name = "Fred", age = 35 },
                    new { name = "Wilma", age = 30 },
                    new { name = "Pebbles", age = 3 }
                }.ToAsyncObject();

        private class DeferredQueryObject : IAsyncDeferredQuery
        {
            private IAsyncObject _remoteObject;

            public DeferredQueryObject(object remoteObject)
            {
                _remoteObject = remoteObject.ToAsyncObject();
            }

            public async Task<IAsyncObject> EvaluateQuery(string query)
            {
                // We'll simulate a service call where we get a query and pass it along to a service where
                // it gets compiled.

                // If this were a real remote service, the compilation would happen on the other side.
                var compiledQuery = Query.Compile(query);
                return (await compiledQuery.Run(_remoteObject)).ToAsyncObject();
            }

            public Task<BifoqlType> GetSchema()
            {
                throw new NotImplementedException();
            }

            public Task<bool> IsEqualTo(IAsyncObject o)
            {
                return Task.FromResult(o is DeferredQueryObject);
            }

        }
    }
}
