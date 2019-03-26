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
    public class AsyncTests : ExprTestBase
    {
        [Fact]
        public async Task DelayedLookup()
        {
            var obj = new MockAsyncLookup(new Dictionary<string, object>() { ["i"] = 5 });

            await RunTestAsync(input: obj, query: "i", expected: 5);
        }

        [Fact]
        public async Task DelayedArray()
        {
            var obj = new MockAsyncLookup(new Dictionary<string, object>() { ["i"] = 5 });

            await RunTestAsync(input: obj, query: "i", expected: 5);
        }
    }
}
