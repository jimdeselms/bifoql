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
    public static class DelayedValue
    {
        public static Task<object> Delayed(this object o)
        {
            return Create(o);
        }

        private static readonly Random _random = new Random();

        // Allows us to do actual async tests. Most tests are actually synchronous.
        private static async Task<object> Create(object value)
        {
            await Task.Delay(_random.Next(5));
            return value;
        }
    }
}
