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
    public class FilterTests
    {
        [Fact]
        public void FilterWithDerivedKey()
        {
            RunTest(new [] { "Ted" }, "@(name: ('T' + 'e' + 'd') ).name", "Ted", "Fred");
        }
        private class NamesAndLetterCount : IBifoqlIndex
        {
            public Func<Task<IBifoqlObject>> this[int index] => throw new NotImplementedException();

            private HashSet<string> _names;

            // we're simulating a service, so this needs to be async.
            public bool NeedsAsync => true;

            public NamesAndLetterCount(params string[] names)
            {
                _names = new HashSet<string>(names);
            }

            public Task<BifoqlType> GetSchema() => Task.FromResult<BifoqlType>(ScalarType.Any);

            public async Task<object> Lookup(IndexArgumentList filter)
            {
                var all = new List<Dictionary<string, object>>();
                foreach (var currName in _names)
                {
                    all.Add(new Dictionary<string, object> { ["name"] = currName, ["length"] = (double)currName.Length});
                }

                var name = await filter.TryGetStringParameter("name");
                if (name != null)
                {
                    return all.Where(e => (string)e["name"] == name);
                }

                return new AsyncError("Not found");
            }

            public Task<bool> IsEqualTo(IBifoqlObject o)
            {
                throw new NotImplementedException();
            }
        }

        private static void RunTest(object expected, string query, params string[] names)
        {
            var inputJson = new NamesAndLetterCount(names);

            var result = Query(inputJson, query).Result;

            var actualJson = JsonConvert.SerializeObject(result);

            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        private static Task<object> Query(object o, string query)
        {
            var queryObj = Bifoql.Query.Compile(query);
            return queryObj.Run(o);
        }
    }
}
