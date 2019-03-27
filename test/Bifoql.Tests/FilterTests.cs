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
    public class FilterTests
    {
        [Fact]
        public void FilterWithDerivedKey()
        {
            RunTest(new [] { "Ted" }, "@(name: ('T' + 'e' + 'd') ).name", "Ted", "Fred");
        }
        private class NamesAndLetterCountAsync : IBifoqlIndex
        {
            private HashSet<string> _names;

            public NamesAndLetterCountAsync(params string[] names)
            {
                _names = new HashSet<string>(names);
            }

            public async Task<object> Lookup(IIndexArgumentList filter)
            {
                var all = new List<Dictionary<string, object>>();
                foreach (var currName in _names)
                {
                    all.Add(new Dictionary<string, object> { ["name"] = currName, ["length"] = (double)currName.Length});
                }

                var name = filter.TryGetStringParameter("name");
                if (name != null)
                {
                    return all.Where(e => (string)e["name"] == name);
                }

                return new AsyncError("Not found");
            }
        }


        private class NamesAndLetterCountSync : IBifoqlIndexSync
        {
            private HashSet<string> _names;

            public NamesAndLetterCountSync(params string[] names)
            {
                _names = new HashSet<string>(names);
            }

            public object Lookup(IIndexArgumentList filter)
            {
                var all = new List<Dictionary<string, object>>();
                foreach (var currName in _names)
                {
                    all.Add(new Dictionary<string, object> { ["name"] = currName, ["length"] = (double)currName.Length});
                }

                var name = filter.TryGetStringParameter("name");
                if (name != null)
                {
                    return all.Where(e => (string)e["name"] == name);
                }

                return new AsyncError("Not found");
            }
        }

        private static void RunTest(object expected, string query, params string[] names)
        {
            var inputObj = new NamesAndLetterCountAsync(names);

            var result = Query(inputObj, query).Result;

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
