using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
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
            AssertCompatible(BifoqlType.ArrayOf(BifoqlType.String), BifoqlType.ArrayOf(BifoqlType.String));
        }

        [Fact]
        public void Inequality()
        {
            AssertNotCompatible(BifoqlType.Number, BifoqlType.String);
            AssertNotCompatible(BifoqlType.ArrayOf(BifoqlType.String), BifoqlType.ArrayOf(BifoqlType.Number));
        }

        [Fact]
        public void Optional()
        {
            AssertCompatible(BifoqlType.Boolean, BifoqlType.Optional(BifoqlType.Boolean));
            AssertCompatible(BifoqlType.Null, BifoqlType.Optional(BifoqlType.Boolean));
            AssertCompatible(BifoqlType.Optional(BifoqlType.Boolean), BifoqlType.Optional(BifoqlType.Boolean));

            AssertNotCompatible(BifoqlType.Optional(BifoqlType.Boolean), BifoqlType.Boolean);
            AssertNotCompatible(BifoqlType.Optional(BifoqlType.Boolean), BifoqlType.Null);
        }

        [Fact]
        public void ArrayWithIncompatibleArray()
        {
            AssertNotCompatible(BifoqlType.ArrayOf(BifoqlType.String), BifoqlType.ArrayOf(BifoqlType.Number));
        }

        [Fact]
        public void ArrayOfOptional()
        {
            AssertCompatible(
                BifoqlType.ArrayOf(BifoqlType.Number), 
                BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.Number)));

            AssertNotCompatible(
                BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.Number)),
                BifoqlType.ArrayOf(BifoqlType.Number)); 
        }

        [Fact]
        public void ArrayAndTupleOfSameType()
        {
            AssertCompatible(
                BifoqlType.Tuple(BifoqlType.Number, BifoqlType.Number), 
                BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.Number)));

            AssertCompatible(
                BifoqlType.Tuple(BifoqlType.Number, BifoqlType.Optional(BifoqlType.Number)), 
                BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.Number)));

            AssertCompatible(
                BifoqlType.Tuple(BifoqlType.Number, BifoqlType.Number), 
                BifoqlType.ArrayOf(BifoqlType.Number));


            AssertNotCompatible(
                BifoqlType.Tuple(BifoqlType.Number, BifoqlType.Optional(BifoqlType.Number)), 
                BifoqlType.ArrayOf(BifoqlType.Number));
        }

        [Fact]
        public void ArrayComparedToTuple()
        {
            AssertNotCompatible(
                BifoqlType.ArrayOf(BifoqlType.String),
                BifoqlType.Tuple(BifoqlType.String)
            );
        }

        [Fact]
        public void TwoTuples()
        {
            AssertCompatible(
                BifoqlType.Tuple(BifoqlType.String, BifoqlType.Number),
                BifoqlType.Tuple(BifoqlType.Optional(BifoqlType.String), BifoqlType.Optional(BifoqlType.Number))
            );

            AssertNotCompatible(
            BifoqlType.Tuple(BifoqlType.String, BifoqlType.Number),
                BifoqlType.Tuple(BifoqlType.Optional(BifoqlType.String), BifoqlType.Optional(BifoqlType.String))
            );

            AssertNotCompatible(
                BifoqlType.Tuple(BifoqlType.String, BifoqlType.String),
                BifoqlType.Tuple(BifoqlType.String, BifoqlType.String, BifoqlType.String)
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
