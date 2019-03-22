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
using Schema = Bifoql.Types.Schema;
using Bifoql.Types;

namespace Bifoql.Tests
{
    public class SchemaTests
    {
        [Fact]
        public void SimpleTypes()
        {
            Assert.Equal("null", new Schema(BifoqlType.Null).BuildDocumentation());
            Assert.Equal("number", new Schema(BifoqlType.Number).BuildDocumentation());
            Assert.Equal("string", new Schema(BifoqlType.String).BuildDocumentation());
            Assert.Equal("any", new Schema(BifoqlType.Any).BuildDocumentation());
        }

        [Fact]
        public void ArrayTypes()
        {
            Assert.Equal("string[]", new Schema(BifoqlType.ArrayOf(BifoqlType.String)).BuildDocumentation());
            Assert.Equal("string[]?", new Schema(BifoqlType.Optional(BifoqlType.ArrayOf(BifoqlType.String))).BuildDocumentation());
            Assert.Equal("string?[]", new Schema(BifoqlType.ArrayOf(BifoqlType.Optional(BifoqlType.String))).BuildDocumentation());
        }

        [Fact]
        public void TupleTypes()
        {
            var expected =
@"[
    number,
    [
        boolean
    ],
    string
]";
            var type = new Schema(BifoqlType.Tuple(BifoqlType.Number, BifoqlType.Tuple(BifoqlType.Boolean), BifoqlType.String));
            Assert.Equal(expected, type.BuildDocumentation());
        }

        [Fact]
        public void MapTypes()
        {
            var expected =
@"{
    x: number,
    burger: [
        boolean
    ],
    street: string
}";
            var type = new Schema(BifoqlType.Map(
                BifoqlType.Property("x", BifoqlType.Number),
                BifoqlType.Property("burger", BifoqlType.Tuple(BifoqlType.Boolean)),
                BifoqlType.Property("street", BifoqlType.String)));
            Assert.Equal(expected, type.BuildDocumentation());
        }

        [Fact]
        public void MapTypesWithDocumentation()
        {
            var expected =
@"{
    // A number
    x: number,

    // A tasty burger
    // yum!
    burger: [
        boolean
    ],

    // The street
    street: string
}";
            var type = new Schema(BifoqlType.Map(
                BifoqlType.Property("x", BifoqlType.Number, "A number"),
                BifoqlType.Property("burger", BifoqlType.Tuple(BifoqlType.Boolean), "A tasty burger\nyum!"),
                BifoqlType.Property("street", BifoqlType.String, "The street")));

            System.IO.File.WriteAllText("d:\\foo.txt", type.BuildDocumentation());
            Assert.Equal(expected, type.BuildDocumentation());
        }

        [Fact]
        public void OptionalTypes()
        {
            Assert.Equal("number?", new Schema(BifoqlType.Optional(BifoqlType.Number)).BuildDocumentation());
        }

        [Fact]
        public void IndexedType()
        {
            var type = new Schema(BifoqlType.Index(BifoqlType.String,
                BifoqlType.IndexParameter("a", BifoqlType.Number, true),
                BifoqlType.IndexParameter("b", BifoqlType.Boolean))
            );
            Assert.Equal("(a?: number, b: boolean) => string", type.BuildDocumentation());
        }

        [Fact]
        public void UnionType()
        {
            var type = new Schema(BifoqlType.Union(BifoqlType.String, BifoqlType.Number, BifoqlType.Null));

            Assert.Equal("string | number | null", type.BuildDocumentation());
        }

        [Fact]
        public void NamedTypeTest()
        {
            var optionalPerson = BifoqlType.Map(BifoqlType.Property("p", 
                BifoqlType.Index(BifoqlType.Named("Person"), 
                    BifoqlType.IndexParameter("id", BifoqlType.Number),
                    BifoqlType.IndexParameter("locale", BifoqlType.String, optional: true))));

            var s = new Schema(optionalPerson)
                .WithNamedType("Address", BifoqlType.Map(BifoqlType.Property("street", BifoqlType.String)))
                .WithNamedType("Person", BifoqlType.Map(BifoqlType.Property("name", BifoqlType.String), BifoqlType.Property("address", BifoqlType.Named("Address"))));

            var schema = 
@"{
    p: (id: number, locale?: string) => Person
}

Address = {
    street: string
}

Person = {
    name: string,
    address: Address
}
";
            Assert.Equal(schema, s.BuildDocumentation());
        }
        [Fact]
        public void NamedTypeWithDocumentationTest()
        {
            var optionalPerson = BifoqlType.Map(BifoqlType.Property("p", 
                BifoqlType.Index(BifoqlType.Named("Person"), 
                    BifoqlType.IndexParameter("id", BifoqlType.Number),
                    BifoqlType.IndexParameter("locale", BifoqlType.String, optional: true))));

            var s = new Schema(optionalPerson)
                .WithNamedType("Person", BifoqlType.Map(BifoqlType.Property("name", BifoqlType.String)), "A human being");

            var schema = 
@"{
    p: (id: number, locale?: string) => Person
}

// A human being
Person = {
    name: string
}
";
            Assert.Equal(schema, s.BuildDocumentation());
        }
    }
}
