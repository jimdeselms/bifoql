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
    public class MockAsyncIndex : IBifoqlIndex
    {
        private readonly string _paramName;
        private readonly IReadOnlyDictionary<string, object> _values;

        public MockAsyncIndex(string paramName, string key, object value)
        {
            _paramName = paramName;
            _values = new Dictionary<string, object>{ [key] = value };
        }
        
        public MockAsyncIndex(string paramName, IReadOnlyDictionary<string, object> values)
        {
            _paramName = paramName;
            _values = values;
        }

        public async Task<object> Lookup(IIndexArgumentList args)
        {
            var key = args.TryGetStringParameter(_paramName);
            if (key == null)
            {
                return null;
            }
            object value;
            if (_values.TryGetValue(key, out value))
            {
                return await value.Delayed();
            }
            else
            {
                return null;
            }
        }
    }
}
