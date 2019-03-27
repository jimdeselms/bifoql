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
    public class MockSyncIndex : IBifoqlIndexSync
    {
        private readonly string _paramName;
        private readonly IReadOnlyDictionary<string, object> _values;

        public MockSyncIndex(string paramName, string key, object value)
        {
            _paramName = paramName;
            _values = new Dictionary<string, object>{ [key] = value };
        }
        
        public MockSyncIndex(string paramName, IReadOnlyDictionary<string, object> values)
        {
            _paramName = paramName;
            _values = values;
        }

        public object Lookup(IIndexArgumentList args)
        {
            var key = args.TryGetStringParameter(_paramName);
            if (key == null)
            {
                return null;
            }
            object value;
            if (_values.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}
