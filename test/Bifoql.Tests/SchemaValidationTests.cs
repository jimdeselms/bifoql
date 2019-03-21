using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using Bifoql.Expressions;
using Bifoql.Types;

namespace Bifoql.Tests
{
    public class SchemaValidationTests
    {
        [Fact]
        public async Task Simple()
        {
            await AssertValidSchema("5", BifoqlType.Number, Schema.Optional(BifoqlType.Number));
        }

        [Fact]
        public async Task SimpleFailure()
        {
            await AssertInvalidSchema("5", BifoqlType.String);
        }

        [Fact]
        public async Task Array()
        {
            await AssertValidSchema("[1, 2]", Schema.ArrayOf(BifoqlType.Number));
            await AssertInvalidSchema("[1, 2]", Schema.ArrayOf(BifoqlType.String));
        }

        [Fact]
        public async Task Tuple()
        {
            await AssertValidSchema("[1, 2]", Schema.Tuple(BifoqlType.Number, BifoqlType.Number));
            await AssertValidSchema("['abc', 2]", Schema.Tuple(BifoqlType.String, BifoqlType.Number));

            await AssertInvalidSchema("[1, 2]", Schema.Tuple(BifoqlType.String, BifoqlType.Number));
            await AssertInvalidSchema("['abc', 1]", Schema.Tuple(BifoqlType.Number, BifoqlType.Number));
        }

        [Fact]
        public async Task Map()
        {
            await AssertValidSchema("{}", Schema.Map());
            await AssertValidSchema("{a: 1}", Schema.Map(Schema.Pair("a", BifoqlType.Number)));

            await AssertInvalidSchema("{a: 1}", Schema.Map(Schema.Pair("a", BifoqlType.String)));
            await AssertInvalidSchema("{a: 1}", Schema.Map(Schema.Pair("b", BifoqlType.Number)));
        }

        [Fact]
        public async Task MapMissingARequiredValue()
        {
            var schema = Schema.Map(Schema.Pair("x", BifoqlType.Number), Schema.Pair("y", BifoqlType.Number));

            await AssertValidSchema("{x: 5, y: 6}", schema);
             await AssertValidSchema("{y: 5, x: 6}", schema);
            await AssertInvalidSchema("{x: 5}", schema, schema);
        }

        [Fact]
        public async Task MapMissingAnOptionalValue()
        {
            var schema = Schema.Map(Schema.Pair("x", BifoqlType.Number), Schema.Pair("y", BifoqlType.Number));

            await AssertValidSchema("{x: 5, y: 6}", schema, schema);
            await AssertInvalidSchema("{x: 5}", schema, schema);
        }

        private static async Task AssertValidSchema(string expression, BifoqlType expectedType, BifoqlType schema=null)
        {
            var query = Query.Compile(expression);
            var obj = await query.Run(null, validateSchema: true);
            var asyncObj = obj.ToBifoqlObject(schema);

            await asyncObj.ToSimpleObject(expectedType ?? schema);
        }

        private static async Task AssertInvalidSchema(string expression, BifoqlType expectedType, BifoqlType schema=null)
        {
            try
            {
                await AssertValidSchema(expression, expectedType, schema);
            }
            catch
            {   
                return;
            }
            throw new Exception("Expected invalid schema");
        }
    }
}
