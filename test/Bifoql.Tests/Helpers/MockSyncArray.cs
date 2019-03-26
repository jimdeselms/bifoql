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
    public class MockSyncArray : IBifoqlArraySync
    {
        private readonly IReadOnlyList<Func<object>> _values;

        public MockSyncArray(IEnumerable<object> values)
        {
            _values = values.Select(v => (Func<object>)(() => v)).ToList();
        }

        public MockSyncArray(params object[] values): this((IEnumerable<object>)values)
        {
        }

        public IReadOnlyList<Func<object>> Items => _values;
    }
}
