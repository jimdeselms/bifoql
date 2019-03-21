using System;
using Xunit;
using Bifoql;
using Bifoql.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Bifoql.Adapters;
using Bifoql.Extensions;

namespace Bifoql.Tests
{
    public class PropertyAdapterTests
    {
        [Fact]
        public void SimpleProperty()
        {
            var tc = new TestClass { String = "Hello" };
            var obj = PropertyAdapter.Create<TestClass>(tc) as IBifoqlMapInternal;

            Assert.Equal("Hello", obj.TryGetValueAsString("String"));
        }

        [Fact]
        public void AsyncStringProperty()
        {
            var tc = new TestClass { AsyncString = (IBifoqlString)"Hello".ToBifoqlObject() };
            var obj = PropertyAdapter.Create<TestClass>(tc) as IBifoqlMapInternal;

            Assert.Equal("Hello", obj.TryGetValueAsString("AsyncString"));
        }

        [Fact]
        public void ObjectFunction()
        {
            var tc = new TestClass { FuncObject = () => "Howdy" };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("Howdy", obj.TryGetValueAsString("FuncObject"));
        }

        [Fact]
        public void AsyncFuncObjectFunction()
        {
            var tc = new TestClass { AsyncFuncObject = () => "Howdy".ToBifoqlObject() };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("Howdy", obj.TryGetValueAsString("AsyncFuncObject"));
        }

        [Fact]
        public void AsyncFuncTaskObjectFunction()
        {
            var tc = new TestClass { AsyncFuncTaskObject = () => Task.FromResult("Howdy".ToBifoqlObject()) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("Howdy", obj.TryGetValueAsString("AsyncFuncTaskObject"));
        }

        [Fact]
        public void LazyObject()
        {
            var tc = new TestClass { LazyObject = new Lazy<object>(() => "Foo") };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("Foo", obj.TryGetValueAsString("LazyObject"));
        }

        [Fact]
        public void AsyncLazyObject()
        {
            var tc = new TestClass { AsyncLazyObject = new Lazy<IBifoqlObject>(() => "Foo".ToBifoqlObject()) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("Foo", obj.TryGetValueAsString("AsyncLazyObject"));
        }

        [Fact]
        public void TaskObject()
        {
            var tc = new TestClass { TaskObject = Task.FromResult<object>(12345) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(12345d, obj.TryGetValueAsNumber("TaskObject"));
        }

        [Fact]
        public void AsyncTaskObject()
        {
            var tc = new TestClass { AsyncTaskObject = Task.FromResult<IBifoqlObject>("HI".ToBifoqlObject()) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("HI", obj.TryGetValueAsString("AsyncTaskObject"));
        }

        [Fact]
        public void FuncTaskObject()
        {
            var tc = new TestClass { FuncTaskObject = () => Task.FromResult<object>(12345) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(12345d, obj.TryGetValueAsNumber("FuncTaskObject"));
        }

        [Fact]
        public void LazyTaskObject()
        {
            var tc = new TestClass { LazyTaskObject = new Lazy<Task<object>>(() => Task.FromResult<object>(12345)) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(12345d, obj.TryGetValueAsNumber("LazyTaskObject"));
        }

        [Fact]
        public void AsyncLazyTaskObject()
        {
            var tc = new TestClass { AsyncLazyTaskObject = new Lazy<Task<IBifoqlObject>>(() => Task.FromResult<IBifoqlObject>("HI".ToBifoqlObject())) };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal("HI", obj.TryGetValueAsString("AsyncLazyTaskObject"));
        }

        [Fact]
        public void SimpleGenericType()
        {
            var list = new List<string> { "Hi" };

            var tc = new TestClass { GenericType = list };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            var listObj = (IBifoqlArrayInternal)obj.TryGetValue("GenericType");
            var hi = listObj[0]().Result.TryGetString();
            Assert.Equal("Hi", hi);
        }

        [Fact]
        public void SimpleAsyncGenericType()
        {
            var list = new List<IBifoqlObject> { "Hi".ToBifoqlObject() };

            var tc = new TestClass { AsyncGenericType = list };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            var listObj = (IBifoqlArrayInternal)obj.TryGetValue("AsyncGenericType");
            var hi = listObj[0]().Result.TryGetString();
            Assert.Equal("Hi", hi);
        }

        [Fact]
        public async Task SimpleNestedAsyncGenericType()
        {
            var list = new List<IBifoqlObject> { "Hey".ToBifoqlObject() };
            var listOfList = new List<IList<IBifoqlObject>> { list };

            var tc = new TestClass { NestedAsyncGenericType = listOfList };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            var listObj = (IBifoqlArrayInternal)obj.TryGetValue("NestedAsyncGenericType");
            var innerListObj = (IBifoqlArrayInternal)await listObj[0]();

            var hi = innerListObj[0]().Result.TryGetString();
            Assert.Equal("Hey", hi);
        }

        [Fact]
        public void NullSimpleProperty()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(AsyncNull.Instance, obj.TryGetValue("String"));
        }

        [Fact]
        public void NullObjectFunction()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(AsyncNull.Instance, obj.TryGetValue("FuncObject"));
        }

        [Fact]
        public void NullLazyObject()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(AsyncNull.Instance, obj.TryGetValue("LazyObject"));
        }

        [Fact]
        public void NullTaskObject()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(new AsyncError("task is null"), obj.TryGetValue("TaskObject"));
        }

        [Fact]
        public void NullFuncTaskObject()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(new AsyncError("task is null"), obj.TryGetValue("FuncTaskObject"));
        }

        [Fact]
        public void NullLazyTaskObject()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(new AsyncError("task is null"), obj.TryGetValue("LazyTaskObject"));
        }

        [Fact]
        public void NullSimpleGenericType()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc);

            Assert.Equal(AsyncNull.Instance, obj.TryGetValue("GenericType"));
        }

        [Fact]
        public void DictOfAsyncObjectTasks()
        {
            var dict = new Dictionary<string, Task<IBifoqlObject>>
            {
                ["a"] = Task.FromResult("ABC".ToBifoqlObject())
            };

            var tc = new TestClass() { DictOfAsyncObjectTasks = dict };
            var obj = PropertyAdapter.Create<TestClass>(tc);

            IBifoqlMapInternal lookup = (IBifoqlMapInternal)obj.TryGetValue("DictOfAsyncObjectTasks");

            Assert.Equal("ABC", lookup.TryGetValueAsString("a"));
        }

        [Fact]
        public void InvalidTypeThrows()
        {
            try
            {
                PropertyAdapter.Create<HasInvalidReturnType>(null);
                Assert.False(true);
            }
            catch
            {
            }
        }

        [Fact]
        public void NullIsAllowed()
        {
            PropertyAdapter.Create<TestClass>(null);
        }

        private class TestClass
        {
            public string String { get; set; }
            public IList<string> GenericType { get; set; }
            public Func<object> FuncObject { get; set; }
            public Lazy<object> LazyObject { get; set; }
            public Task<object> TaskObject { get; set;  }
            public Func<Task<object>> FuncTaskObject { get; set; }
            public Lazy<Task<object>> LazyTaskObject { get; set; }

            public IBifoqlString AsyncString { get; set; }
            public Func<IBifoqlObject> AsyncFuncObject { get; set; }
            public Func<Task<IBifoqlObject>> AsyncFuncTaskObject { get; set; }
            public Lazy<IBifoqlObject> AsyncLazyObject { get; set; }
            public Lazy<Task<IBifoqlObject>> AsyncLazyTaskObject { get; set; }
            public Task<IBifoqlObject> AsyncTaskObject { get; set; }
            public IList<IBifoqlObject> AsyncGenericType { get; set; }
            public IList<IList<IBifoqlObject>> NestedAsyncGenericType { get; set; }
            public IDictionary<string, Task<IBifoqlObject>> DictOfAsyncObjectTasks { get; set; }
        }

        private class HasInvalidReturnType
        {
            public Func<string> Foo { get; set; }
        }
    }
}
