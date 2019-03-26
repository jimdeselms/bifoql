using System;
using System.Linq;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bifoql.Tests.Helpers
{
    public class MockAsyncMap : IBifoqlMap
    {
        private readonly IReadOnlyDictionary<string, Func<Task<object>>> _values;

        public MockAsyncMap(string key, object value)
        {
            _values = new Dictionary<string, Func<Task<object>>>
            {
                [key] = (Func<Task<object>>)(() => value.Delayed())
            };
        }

        public MockAsyncMap(IReadOnlyDictionary<string, object> values)
        {
            _values = values.ToDictionary(
                v => v.Key,
                v => (Func<Task<object>>)(() => v.Value.Delayed()));
        }

        public IReadOnlyDictionary<string, Func<Task<object>>> Items => _values;

        public bool TryGetValue(string key, out Func<Task<object>> result)
        {
            return _values.TryGetValue(key, out result);
        }
    }
}
