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
    public class MockAsyncArray : IBifoqlArraySync
    {
        private readonly IReadOnlyList<Func<Task<object>>> _values;

        public MockAsyncArray(IEnumerable<object> values)
        {
            _values = values.Select(v => 
                (Func<Task<object>>)(() => v.Delayed())).ToList();
        }

        public MockAsyncArray(params object[] values): this((IEnumerable<object>)values)
        {
        }

        public IReadOnlyList<Func<object>> Items => _values;
    }
}
