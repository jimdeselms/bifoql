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
            Assert.Equal("null", Schema.Null.ToString());
            Assert.Equal("number", Schema.Number.ToString());
            Assert.Equal("string", Schema.String.ToString());
            Assert.Equal("any", Schema.Any.ToString());
        }

        [Fact]
        public void ArrayTypes()
        {
            Assert.Equal("string[]", Schema.ArrayOf(Schema.String).ToString());
            Assert.Equal("string[]?", Schema.Optional(Schema.ArrayOf(Schema.String)).ToString());
            Assert.Equal("string?[]", Schema.ArrayOf(Schema.Optional(Schema.String)).ToString());
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
            var type = Schema.Tuple(Schema.Number, Schema.Tuple(Schema.Boolean), Schema.String);
            Assert.Equal(expected, type.ToString());
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
            var type = Schema.Map(
                Schema.Pair("x", Schema.Number),
                Schema.Pair("burger", Schema.Tuple(Schema.Boolean)),
                Schema.Pair("street", Schema.String));
            Assert.Equal(expected, type.ToString());
        }

        [Fact]
        public void OptionalTypes()
        {
            Assert.Equal("number?", Schema.Optional(Schema.Number).ToString());
        }

        [Fact]
        public void IndexedType()
        {
            var type = Schema.Index(Schema.String,
                Schema.IndexParameter("a", Schema.Number, true),
                Schema.IndexParameter("b", Schema.Boolean)
            );
            Assert.Equal("(a?: number, b: boolean) => string", type.ToString());
        }

        [Fact]
        public void UnionType()
        {
            var type = Schema.Union(Schema.String, Schema.Number, Schema.Null);

            Assert.Equal("string | number | null", type.ToString());
        }

        [Fact]
        public void NamedTypeTest()
        {
            var address = Schema.Named("Address", Schema.Map(Schema.Pair("street", Schema.String)));
            var person = Schema.Named("Person", Schema.Map(Schema.Pair("name", Schema.String), Schema.Pair("address", address)));
            var optionalPerson = Schema.Map(Schema.Pair("p", 
                Schema.Index(person, 
                    Schema.IndexParameter("id", Schema.Number),
                    Schema.IndexParameter("locale", Schema.String, optional: true))));

            var schema = 
@"{
    p: (id: number, locale?: string) => Person
}

Address {
    street: string
}

Person {
    name: string,
    address: Address
}
";
            Assert.Equal(schema, optionalPerson.ToString());
        }

        [Fact]
        public void DuplicateNamedReferences()
        {
            // We could build in protection to prevent there from being more than one named type with the same
            // name, but for now, we'll just make sure they get filtered out properly.
            var n1 = Schema.Named("n1", Schema.Map(Schema.Pair("x", Schema.String)));
            var n2 = Schema.Named("n1", Schema.Map(Schema.Pair("x", Schema.String)));
            var n3 = Schema.Named("n2", Schema.Map(Schema.Pair("x", Schema.String)));
            var n4 = Schema.Named("n2", Schema.Map(Schema.Pair("x", Schema.String)));
            var n5 = Schema.Named("n3", Schema.Map(Schema.Pair("x", Schema.String)));
            var n6 = Schema.Named("n3", Schema.Map(Schema.Pair("x", Schema.String)));
            
            var type = Schema.Map(
                Schema.Pair("a1", n1),
                Schema.Pair("a2", n2),
                Schema.Pair("a3", n3),
                Schema.Pair("a4", n4),
                Schema.Pair("a5", n5),
                Schema.Pair("a6", n6)
            );
            
            var schema =
@"{
    a1: n1,
    a2: n1,
    a3: n2,
    a4: n2,
    a5: n3,
    a6: n3
}

n1 {
    x: string
}

n2 {
    x: string
}

n3 {
    x: string
}
";
            Assert.Equal(schema, type.ToString());

        }
    }
}
