namespace Bifoql.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Types;
    using Bifoql;
    using Bifoql.Extensions;
    using Xunit;

    public class ObjectConverterTests
    {
        [Fact]
        public async Task Int()
        {
            var obj = 5.ToAsyncObject();
            var actualValue = await ((IBifoqlNumber)obj).Value;
            Assert.Equal(5, actualValue);
        }

        [Fact]
        public async Task String()
        {
            var obj = "Hello".ToAsyncObject();
            var actualValue = await ((IBifoqlString)obj).Value;
            Assert.Equal("Hello", actualValue);
        }

        [Fact]
        public async Task Boolean()
        {
            var obj = true.ToAsyncObject();
            var actualValue = await ((IBifoqlBoolean)obj).Value;
            Assert.Equal(true, actualValue);
        }

        [Fact]
        public async Task Array()
        {
            var obj = (new [] { 1, 2, 3 }).ToAsyncObject();
            var list = (IBifoqlArray)obj;

            Assert.Equal(3, list.Count);

            var second = await list[1]();
            Assert.Equal(2, await ((IBifoqlNumber)second).Value);
        }

        [Fact]
        public async Task ListWithAsyncItem()
        {
            var num = 5.ToAsyncObject();
            var arr = new object[]{ num };

            var obj = (IBifoqlArray)arr.ToAsyncObject();

            Assert.Equal(1, obj.Count);

            var first = await obj[0]();
            Assert.Equal(5, await ((IBifoqlNumber)first).Value);
        }

        [Fact]
        public async Task ListWithAsyncDict()
        {
            var dict = new AsyncDictThatBlowsUp();
            var arr = new object[] { dict };
            var obj = (IBifoqlArray)arr.ToAsyncObject();

            Assert.Equal(1, obj.Count);

            var first = await obj[0]();
            Assert.Same(dict, first);
        }

        [Fact]
        public async Task DictWithAsyncDict()
        {
            var dict = new AsyncDictThatBlowsUp();
            var arr = new Dictionary<string, object> { ["dict"] = dict };
            var obj = (IBifoqlMap)arr.ToAsyncObject();

            Assert.Equal(1, obj.Count);

            var first = await obj["dict"]();
            Assert.Same(dict, first);
        }

        [Fact]
        public async Task NestedAnonymousObject()
        {
            var dict = new AsyncDictThatBlowsUp();
            var anon = new {
                foo = new { bar = dict }
            };

            var obj = (IBifoqlMap)anon.ToAsyncObject();

            var foo = (IBifoqlMap)(await obj["foo"]());
            var bar = await foo["bar"]();

            Assert.Same(dict, bar);
        }


        [Fact]
        public async Task NestedDict()
        {
            // TODO... I shouldn't have to "ToAsyncObject" the inner anonymous dict.
            var dict = new AsyncDictThatBlowsUp();
            var anon = new Dictionary<string, object>{
                ["foo"] = (new Dictionary<string, object> { ["bar"] = dict })
            };

            var obj = (IBifoqlMap)anon.ToAsyncObject();

            var foo = (IBifoqlMap)(await obj["foo"]());
            var bar = await foo["bar"]();

            Assert.Same(dict, bar);
        }

        [Fact]
        public async Task ObjectWithAsyncDict()
        {
            var dict = new AsyncDictThatBlowsUp();
            var arr = new { dict = dict };
            var obj = (IBifoqlMap)arr.ToAsyncObject();

            Assert.Equal(1, obj.Count);

            var first = await obj["dict"]();
            Assert.Same(dict, first);
        }

        private class AsyncDictThatBlowsUp : IBifoqlMap
        {
            public Func<Task<IBifoqlObject>> this[string key] => throw new NotImplementedException();

            public IEnumerable<string> Keys => throw new NotImplementedException();

            public IEnumerable<Func<Task<IBifoqlObject>>> Values => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public Task<BifoqlType> GetSchema() => Task.FromResult<BifoqlType>(BifoqlType.Any);

            public bool ContainsKey(string key)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<string, Func<Task<IBifoqlObject>>>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public Task<bool> IsEqualTo(IBifoqlObject o)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(string key, out Func<Task<IBifoqlObject>> value)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}