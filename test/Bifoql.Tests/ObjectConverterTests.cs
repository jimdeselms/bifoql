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

    internal class ObjectConverterTests
    {
        [Fact]
        public async Task Int()
        {
            var obj = 5.ToBifoqlObject();
            var actualValue = await ((IBifoqlNumber)obj).Value;
            Assert.Equal(5, actualValue);
        }

        [Fact]
        public async Task String()
        {
            var obj = "Hello".ToBifoqlObject();
            var actualValue = await ((IBifoqlString)obj).Value;
            Assert.Equal("Hello", actualValue);
        }

        [Fact]
        public async Task Boolean()
        {
            var obj = true.ToBifoqlObject();
            var actualValue = await ((IBifoqlBoolean)obj).Value;
            Assert.Equal(true, actualValue);
        }

        [Fact]
        public async Task Array()
        {
            var obj = (new [] { 1, 2, 3 }).ToBifoqlObject();
            var list = (IBifoqlArrayInternal)obj;

            Assert.Equal(3, list.Count);

            var second = await list[1]();
            Assert.Equal(2, await ((IBifoqlNumber)second).Value);
        }

        [Fact]
        public async Task ListWithAsyncItem()
        {
            var num = 5.ToBifoqlObject();
            var arr = new object[]{ num };

            var obj = (IBifoqlArrayInternal)arr.ToBifoqlObject();

            Assert.Equal(1, obj.Count);

            var first = await obj[0]();
            Assert.Equal(5, await ((IBifoqlNumber)first).Value);
        }

        [Fact]
        public async Task ListWithAsyncDict()
        {
            var dict = new AsyncDictThatBlowsUp();
            var arr = new object[] { dict };
            var obj = (IBifoqlArrayInternal)arr.ToBifoqlObject();

            Assert.Equal(1, obj.Count);

            var first = await obj[0]();
            Assert.Same(dict, first);
        }

        [Fact]
        public async Task DictWithAsyncDict()
        {
            var dict = new AsyncDictThatBlowsUp();
            var arr = new Dictionary<string, object> { ["dict"] = dict };
            var obj = (IBifoqlMapInternal)arr.ToBifoqlObject();

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

            var obj = (IBifoqlMapInternal)anon.ToBifoqlObject();

            var foo = (IBifoqlMapInternal)(await obj["foo"]());
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

            var obj = (IBifoqlMapInternal)anon.ToBifoqlObject();

            var foo = (IBifoqlMapInternal)(await obj["foo"]());
            var bar = await foo["bar"]();

            Assert.Same(dict, bar);
        }

        [Fact]
        public async Task ObjectWithAsyncDict()
        {
            var dict = new AsyncDictThatBlowsUp();
            var arr = new { dict = dict };
            var obj = (IBifoqlMapInternal)arr.ToBifoqlObject();

            Assert.Equal(1, obj.Count);

            var first = await obj["dict"]();
            Assert.Same(dict, first);
        }

        [Fact]
        public async Task AsyncMap()
        {
            var map = (IBifoqlMapInternal)new MyAsyncMap().ToBifoqlObject();
            var value = await map["foo"]();
            Assert.Equal("Hello", await value.ToSimpleObject());
        }

        [Fact]
        public async Task AsyncArray()
        {
            var array = (IBifoqlArrayInternal)new MyAsyncArray().ToBifoqlObject();

            var one = await array[0]();
            var two = await array[1]();

            Assert.Equal(1, await one.ToSimpleObject());
            Assert.Equal(2, await two.ToSimpleObject());
        }

        [Fact]
        public async Task SyncMap()
        {
            var map = (IBifoqlMapInternal)new MySyncMap().ToBifoqlObject();
            var value = await map["foo"]();
            Assert.Equal("Hello", await value.ToSimpleObject());
        }

        [Fact]
        public async Task SyncArray()
        {
            var array = (IBifoqlArrayInternal)new MySyncArray().ToBifoqlObject();

            var one = await array[0]();
            var two = await array[1]();

            Assert.Equal(1, await one.ToSimpleObject());
            Assert.Equal(2, await two.ToSimpleObject());
        }

        private class MySyncMap : IBifoqlMapSync
        {
            public IReadOnlyDictionary<string, Func<object>> Items => new Dictionary<string, Func<object>>
            {
                ["foo"] = () => "Hello"
            };
        }

        private class MySyncArray : IBifoqlArraySync
        {
            public IReadOnlyList<Func<object>> Items => new List<Func<object>>
            {
                () => 1,
                () => 2,
            };
        }

        private class MyAsyncMap : IBifoqlMap
        {
            public IReadOnlyDictionary<string, Func<Task<object>>> Items => new Dictionary<string, Func<Task<object>>>
            {
                ["foo"] = () => Task.FromResult<object>("Hello")
            };
        }

        private class MyAsyncArray : IBifoqlArray
        {
            public IReadOnlyList<Func<Task<object>>> Items => new List<Func<Task<object>>>
            {
                () => Task.FromResult<object>(1),
                () => Task.FromResult<object>(2),
            };
        }


        private class AsyncDictThatBlowsUp : IBifoqlMapInternal
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