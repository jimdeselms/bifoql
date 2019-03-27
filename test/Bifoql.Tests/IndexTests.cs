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
        
        protected class Greeting : IBifoqlIndexSync
        {
            public object Lookup(IIndexArgumentList args)
            {
                var id = args.TryGetStringParameter("key");
                if (id == "hello")
                {
                    return "world";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
