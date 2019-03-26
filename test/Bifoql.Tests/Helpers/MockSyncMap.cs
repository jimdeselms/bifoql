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
    public class MockSyncMap : IBifoqlMapSync
    {
        private readonly IReadOnlyDictionary<string, Func<object>> _values;

        public MockSyncMap(string key, object value)
        {
            _values = new Dictionary<string, Func<object>>
            {
                [key] = (Func<object>)(() => value)
            };
        }

        public MockSyncMap(IReadOnlyDictionary<string, object> values)
        {
            _values = values.ToDictionary(
                v => v.Key,
                v => (Func<object>)(() => v.Value));
        }

        public IReadOnlyDictionary<string, Func<object>> Items => _values;

        public bool TryGetValue(string key, out Func<object> result)
        {
            return _values.TryGetValue(key, out result);
        }
    }
}
