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
    public class VariableTests : ExprTestBase
    {
        [Fact]
        public void CantReferenceUndefinedVariable()
        {
            RunTest(
                expected: "<error: (1, 9) reference to undefined variable '$x'>",
                query: "$x = &(*$x); *$x");
        }
    }
}
