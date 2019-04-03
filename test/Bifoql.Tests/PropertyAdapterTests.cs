using System;
using Xunit;
using Bifoql;
using Bifoql.Tests.Helpers;
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
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal("Hello", obj.TryGetValueAsString("String"));
        }

        [Fact]
        public void SimpleNumber()
        {
            var tc = new TestClass { Number = 123.0 };
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal(123.0, obj.TryGetValueAsNumber("Number"));
        }

        [Fact]
        public void SimpleInteger()
        {
            var tc = new TestClass { Integer = 456 };
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal(456, (int)obj.TryGetValueAsNumber("Integer"));
        }

        [Fact]
        public void SimpleNamedType()
        {
            var tc = new TestClass { NamedType = new Person { Name = "Bill" }};
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal("Bill", obj.TryGetValue("NamedType").TryGetValueAsString("Name"));
        }

        [Fact]
        public void SimpleNamedTypeTask()
        {
            var tc = new TestClass { NamedTypeTask = Task.FromResult(new Person { Name = "Bill" })};
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal("Bill", obj.TryGetValue("NamedTypeTask").TryGetValueAsString("Name"));
        }

        [Fact]
        public void SimpleObjectTask()
        {
            var tc = new TestClass { TaskObject = Task.FromResult<object>(new Person { Name = "Bill" })};
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal("Bill", obj.TryGetValue("TaskObject").TryGetValueAsString("Name"));
        }

        [Fact]
        public void AsyncStringProperty()
        {
            var tc = new TestClass { AsyncString = (IBifoqlString)"Hello".ToBifoqlObject() };
            var obj = PropertyAdapter.Create<TestClass>(tc, null) as IBifoqlLookupInternal;

            Assert.Equal("Hello", obj.TryGetValueAsString("AsyncString"));
        }

        [Fact]
        public void SimpleObject()
        {
            var tc = new TestClass { SimpleObject = new { Name = "Fred"} };
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            Assert.Equal("Fred", obj.TryGetValue("SimpleObject").TryGetValueAsString("Name"));
        }

        [Fact]
        public void TaskObject()
        {
            var tc = new TestClass { TaskObject = Task.FromResult<object>(12345) };
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            Assert.Equal(12345d, obj.TryGetValueAsNumber("TaskObject"));
        }

        [Fact]
        public void AsyncTaskObject()
        {
            var tc = new TestClass { AsyncTaskObject = Task.FromResult<IBifoqlObject>("HI".ToBifoqlObject()) };
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            Assert.Equal("HI", obj.TryGetValueAsString("AsyncTaskObject"));
        }

        [Fact]
        public void SimpleGenericType()
        {
            var list = new List<string> { "Hi" };

            var tc = new TestClass { GenericType = list };
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            var listObj = (IBifoqlArrayInternal)obj.TryGetValue("GenericType");
            var hi = listObj[0]().Result.TryGetString();
            Assert.Equal("Hi", hi);
        }

        [Fact]
        public void SimpleAsyncGenericType()
        {
            var list = new List<IBifoqlObject> { "Hi".ToBifoqlObject() };

            var tc = new TestClass { AsyncGenericType = list };
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

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
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            var listObj = (IBifoqlArrayInternal)obj.TryGetValue("NestedAsyncGenericType");
            var innerListObj = (IBifoqlArrayInternal)await listObj[0]();

            var hi = innerListObj[0]().Result.TryGetString();
            Assert.Equal("Hey", hi);
        }

        [Fact]
        public void NullSimpleProperty()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            Assert.Equal(AsyncNull.Instance, obj.TryGetValue("String"));
        }

        [Fact]
        public void NullTaskObject()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            Assert.Equal(new AsyncError("task is null"), obj.TryGetValue("TaskObject"));
        }

        [Fact]
        public void NullSimpleGenericType()
        {
            var tc = new TestClass();
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

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
            var obj = PropertyAdapter.Create<TestClass>(tc, null);

            IBifoqlLookupInternal lookup = (IBifoqlLookupInternal)obj.TryGetValue("DictOfAsyncObjectTasks");

            Assert.Equal("ABC", lookup.TryGetValueAsString("a"));
        }

        [Fact]
        public void NullIsAllowed()
        {
            PropertyAdapter.Create<TestClass>(null, null);
        }

        private class TestClass
        {
            public string String { get; set; }
            public double Number { get; set; }
            public int Integer{ get; set; }

            public object SimpleObject { get; set; }

            public Person NamedType { get; set; }
            public Task<Person> NamedTypeTask { get; set; }
            
            public IList<string> GenericType { get; set; }
            public Task<object> TaskObject { get; set;  }
            public IBifoqlString AsyncString { get; set; }
            public Task<IBifoqlObject> AsyncTaskObject { get; set; }
            public IList<IBifoqlObject> AsyncGenericType { get; set; }
            public IList<IList<IBifoqlObject>> NestedAsyncGenericType { get; set; }
            public IDictionary<string, Task<IBifoqlObject>> DictOfAsyncObjectTasks { get; set; }
        }

        private class HasInvalidReturnType
        {
            public Func<string> Foo { get; set; }
        }

        private class Person
        {
            public string Name { get; set; }
        }
    }
}
