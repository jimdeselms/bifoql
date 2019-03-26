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
    public class MockAsyncLookup : IBifoqlLookup
    {
        private readonly IReadOnlyDictionary<string, object> _values;

        public MockAsyncLookup(string key, object value)
        {
            _values = new Dictionary<string, object>{ [key] = value };
        }
        
        public MockAsyncLookup(IReadOnlyDictionary<string, object> values)
        {
            _values = values;
        }
        
        public bool TryGetValue(string key, out Func<Task<object>> result)
        {
            object value;
            if (_values.TryGetValue(key, out value))
            {
                Task<object> task = value.Delayed();
                result = () => task;
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
