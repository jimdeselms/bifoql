using System;
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
    public class MockSyncLookup : IBifoqlLookupSync
    {
        private readonly IReadOnlyDictionary<string, object> _values;

        public MockSyncLookup(string key, object value)
        {
            _values = new Dictionary<string, object>{ [key] = value };
        }
        
        public MockSyncLookup(IReadOnlyDictionary<string, object> values)
        {
            _values = values;
        }
        public bool TryGetValue(string key, out Func<object> result)
        {
            object value;
            if (_values.TryGetValue(key, out value))
            {
                result = () => value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
