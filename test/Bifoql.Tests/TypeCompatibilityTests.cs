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
    public class TypeCompatibilityTests
    {
        [Fact]
        public void Equality()
        {
            AssertCompatible(BifoqlType.String, BifoqlType.String);
            AssertCompatible(Schema.ArrayOf(BifoqlType.String), Schema.ArrayOf(BifoqlType.String));
        }

        [Fact]
        public void Inequality()
        {
            AssertNotCompatible(BifoqlType.Number, BifoqlType.String);
            AssertNotCompatible(Schema.ArrayOf(BifoqlType.String), Schema.ArrayOf(BifoqlType.Number));
        }

        [Fact]
        public void Optional()
        {
            AssertCompatible(BifoqlType.Boolean, Schema.Optional(BifoqlType.Boolean));
            AssertCompatible(BifoqlType.Null, Schema.Optional(BifoqlType.Boolean));
            AssertCompatible(Schema.Optional(BifoqlType.Boolean), Schema.Optional(BifoqlType.Boolean));

            AssertNotCompatible(Schema.Optional(BifoqlType.Boolean), BifoqlType.Boolean);
            AssertNotCompatible(Schema.Optional(BifoqlType.Boolean), BifoqlType.Null);
        }

        [Fact]
        public void ArrayWithIncompatibleArray()
        {
            AssertNotCompatible(Schema.ArrayOf(BifoqlType.String), Schema.ArrayOf(BifoqlType.Number));
        }

        [Fact]
        public void ArrayOfOptional()
        {
            AssertCompatible(
                Schema.ArrayOf(BifoqlType.Number), 
                Schema.ArrayOf(Schema.Optional(BifoqlType.Number)));

            AssertNotCompatible(
                Schema.ArrayOf(Schema.Optional(BifoqlType.Number)),
                Schema.ArrayOf(BifoqlType.Number)); 
        }

        [Fact]
        public void ArrayAndTupleOfSameType()
        {
            AssertCompatible(
                Schema.Tuple(BifoqlType.Number, BifoqlType.Number), 
                Schema.ArrayOf(Schema.Optional(BifoqlType.Number)));

            AssertCompatible(
                Schema.Tuple(BifoqlType.Number, Schema.Optional(BifoqlType.Number)), 
                Schema.ArrayOf(Schema.Optional(BifoqlType.Number)));

            AssertCompatible(
                Schema.Tuple(BifoqlType.Number, BifoqlType.Number), 
                Schema.ArrayOf(BifoqlType.Number));


            AssertNotCompatible(
                Schema.Tuple(BifoqlType.Number, Schema.Optional(BifoqlType.Number)), 
                Schema.ArrayOf(BifoqlType.Number));
        }

        [Fact]
        public void ArrayComparedToTuple()
        {
            AssertNotCompatible(
                Schema.ArrayOf(BifoqlType.String),
                Schema.Tuple(BifoqlType.String)
            );
        }

        [Fact]
        public void TwoTuples()
        {
            AssertCompatible(
                Schema.Tuple(BifoqlType.String, BifoqlType.Number),
                Schema.Tuple(Schema.Optional(BifoqlType.String), Schema.Optional(BifoqlType.Number))
            );

            AssertNotCompatible(
            Schema.Tuple(BifoqlType.String, BifoqlType.Number),
                Schema.Tuple(Schema.Optional(BifoqlType.String), Schema.Optional(BifoqlType.String))
            );

            AssertNotCompatible(
                Schema.Tuple(BifoqlType.String, BifoqlType.String),
                Schema.Tuple(BifoqlType.String, BifoqlType.String, BifoqlType.String)
            );
        }

        private void AssertCompatible(BifoqlType moreSpecific, BifoqlType lessSpecific)
        {
            Assert.True(TypeCompatibilityChecker.IsCompatible(moreSpecific, lessSpecific));
        }

        private void AssertNotCompatible(BifoqlType moreSpecific, BifoqlType lessSpecific)
        {
            Assert.False(TypeCompatibilityChecker.IsCompatible(moreSpecific, lessSpecific));
        }
    }
}
