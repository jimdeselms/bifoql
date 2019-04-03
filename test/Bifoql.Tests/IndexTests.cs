using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bifoql.Tests
{
    public class IndexTests : ExprTestBase
    {
        [Fact]
        public void IndexReferencingContextWithChain()
        {
            RunTest(
                expected: "world",
                query: "'hello' | $.index(key: @)",
                input: new { foo="howdy", index = new Greeting() }.ToBifoqlObject());
        }

        [Fact]
        public void IndexReferencingContext()
        {
            RunTest(
                expected: "world",
                query: "'hello' | $(key: @)",
                input: new Greeting().ToBifoqlObject());
        }
        
        [Fact]
        public void IndexWithDefaultValue()
        {
            RunTest(
                expected: "bar",
                query: "$()",
                input: new Greeting().ToBifoqlObject());
        }

        [Fact]
        public void IndexAcceptsIdByItselfAsBooleanTrue()
        {
            RunTest(
                expected: "WORLD",
                query: "$(key: 'hello', capitalize)",
                input: new Greeting().ToBifoqlObject());
            RunTest(
                expected: "WORLD",
                query: "$(capitalize, key: 'hello')",
                input: new Greeting().ToBifoqlObject());
            RunTest(
                expected: "BAR",
                query: "$(capitalize)",
                input: new Greeting().ToBifoqlObject());
        }        
        protected class Greeting : IBifoqlIndexSync
        {
            public object Lookup(IIndexArgumentList args)
            {
                var id = args.TryGetStringParameter("key");
                var capitalize = args.TryGetBooleanParameter("capitalize") == true;

                id = id ?? "foo";

                string result = null;

                if (id == "hello")
                {
                    result = "world";
                }
                else if (id == "foo")
                {
                    result = "bar";
                }
                else
                {
                    result = null;
                }

                return capitalize ? result?.ToUpper() : result;
            }
        }
    }
}
