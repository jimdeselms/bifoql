using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Bifoql.Expressions;
using Bifoql.Types;

namespace Bifoql.Tests
{
    public class ArrayTypeInfererTests
    {
        [Fact]
        public void NoEntries()
        {
            // An array with zero entries in it is an empty tuple.p
            Test(Schema.Tuple(), new BifoqlType[0]);
        }

        [Fact]
        public void OneType()
        {
            Test(Schema.ArrayOf(BifoqlType.String), BifoqlType.String);
        }
        
        [Fact]
        public void TwoSameTypes()
        {
            Test(Schema.ArrayOf(BifoqlType.String), BifoqlType.String, BifoqlType.String);
        }
        
        [Fact]
        public void TwoDifferentTypes()
        {
            Test(Schema.Tuple(BifoqlType.String, BifoqlType.Number), 
                BifoqlType.String, 
                BifoqlType.Number);
        }
        
        [Fact]
        public void TwoSameTypesOneNullMakesOptional()
        {
            Test(Schema.ArrayOf(Schema.Optional(BifoqlType.String)), 
                BifoqlType.String,
                BifoqlType.Null);

            Test(Schema.ArrayOf(Schema.Optional(BifoqlType.String)), 
                BifoqlType.Null,
                BifoqlType.String);
        }

        [Fact]
        public void TwoSameTypesOneOptionalMakesOptional()
        {
            Test(Schema.ArrayOf(Schema.Optional(BifoqlType.String)), 
                BifoqlType.String,
                Schema.Optional(BifoqlType.String));

            Test(Schema.ArrayOf(Schema.Optional(BifoqlType.String)), 
                Schema.Optional(BifoqlType.String),
                BifoqlType.String);
        }

        [Fact]
        public void TwoDifferentOptionalTypes()
        {
            Test(Schema.Tuple(Schema.Optional(BifoqlType.String), Schema.Optional(BifoqlType.Boolean)),
                Schema.Optional(BifoqlType.String),
                Schema.Optional(BifoqlType.Boolean));
        }

        [Fact]
        public void TwoDifferentOptionalTypesOneOptional()
        {
            Test(Schema.Tuple(BifoqlType.String, Schema.Optional(BifoqlType.Boolean)),
                BifoqlType.String,
                Schema.Optional(BifoqlType.Boolean));
        }

        [Fact]
        public void ArrayAndCompatibleTuple()
        {
            Test(Schema.ArrayOf(Schema.ArrayOf(BifoqlType.Number)),
                Schema.ArrayOf(BifoqlType.Number),
                Schema.Tuple(BifoqlType.Number, BifoqlType.Number));
        }

        private void Test(BifoqlType expected, params BifoqlType[] entries)
        {
            var actual = ArrayTypeInferer.InferArrayType(entries);
            Assert.True(expected.Equals(actual));
        }

        private static async Task<object> CreateObj(string expr, BifoqlType schema)
        {
            var query = Query.Compile(expr);
            var obj = await query.Run(null);
            return obj.ToBifoqlObject(schema);
        }
    }
}
