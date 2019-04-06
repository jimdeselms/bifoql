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
    public class PipeTests : ExprTestBase
    {
        [Fact]
        public void PipeArrayToMany()
        {
            RunTest(
                expected: new [] { 2, 4, 6, 8 },
                input: new [] { 1, 2, 3, 4 },
                query: "@ |< @*2"
            );
        }

        [Fact]
        public void RootObjectVariableAfterPipe()
        {

            RunTest(expected: 5, input: 5, query: "2|$");
        }

        [Fact]
        public void RootObjectVariableAfterPipeToMany()
        {

            RunTest(expected: new [] { 5, 5}, input: 5, query: "[1, 2] |< $");
        }
    }
}
