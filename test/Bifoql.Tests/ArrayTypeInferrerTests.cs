using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
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
            Test(BifoqlType.Tuple(), new BifoqlType[0]);
        }

        [Fact]
        public void OneType()
        {
            Test(BifoqlType.ArrayOf(BifoqlType.String), BifoqlType.String);
        }
        
        [Fact]
        public void TwoSameTypes()
        {
            Test(BifoqlType.ArrayOf(BifoqlType.String), BifoqlType.String, BifoqlType.String);
        }
        
        [Fact]
        public void TwoDifferentTypes()
        {
            Test(BifoqlType.Tuple(BifoqlType.String, BifoqlType.Number), 
                BifoqlType.String, 
                BifoqlType.Number);
        }
        
        [Fact]
        public void TwoSameTypesOneNullMakesOptional()
        {
            Test(BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.String)), 
                BifoqlType.String,
                BifoqlType.Null);

            Test(BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.String)), 
                BifoqlType.Null,
                BifoqlType.String);
        }

        [Fact]
        public void TwoSameTypesOneOptionalMakesOptional()
        {
            Test(BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.String)), 
                BifoqlType.String,
                BifoqlType.Optional(BifoqlType.String));

            Test(BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.String)), 
                BifoqlType.Optional(BifoqlType.String),
                BifoqlType.String);
        }

        [Fact]
        public void TwoDifferentOptionalTypes()
        {
            Test(BifoqlType.Tuple(BifoqlType.Optional(BifoqlType.String), BifoqlType.Optional(BifoqlType.Boolean)),
                BifoqlType.Optional(BifoqlType.String),
                BifoqlType.Optional(BifoqlType.Boolean));
        }

        [Fact]
        public void TwoDifferentOptionalTypesOneOptional()
        {
            Test(BifoqlType.Tuple(BifoqlType.String, BifoqlType.Optional(BifoqlType.Boolean)),
                BifoqlType.String,
                BifoqlType.Optional(BifoqlType.Boolean));
        }

        [Fact]
        public void ArrayAndCompatibleTuple()
        {
            Test(BifoqlType.ArrayOf(BifoqlType.ArrayOf(BifoqlType.Number)),
                BifoqlType.ArrayOf(BifoqlType.Number),
                BifoqlType.Tuple(BifoqlType.Number, BifoqlType.Number));
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
