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
using Bifoql.Adapters;

namespace Bifoql.Tests
{
    // TODO: This is a place to write tests for all of the different ways that we can ToBifoqlObject a thing.
    public class AdapterTests : ExprTestBase
    {
        [Fact]
        public void BifoqlArraySyncTest()
        {
            RunTest(
                input: new ChildList(),
                expected: "Fred",
                query: "@[0].name");
        }

        private class Parent
        {
            public object name => "Fred";
            public object children => new ChildList();
        }

        private class ChildList : IBifoqlArraySync
        {
            private readonly Lazy<IReadOnlyList<Func<object>>> _items;

            public ChildList()
            {
                _items = new Lazy<IReadOnlyList<Func<object>>>(LoadItems);
            }

            private IReadOnlyList<Func<object>> LoadItems()
            {
                var list = new List<Func<object>>();
                list.Add(() => new Parent());
                list.Add(() => new Parent());
                list.Add(() => new Parent());
                return list;
            }

            public IReadOnlyList<Func<object>> Items => _items.Value;
        }

    }
}
