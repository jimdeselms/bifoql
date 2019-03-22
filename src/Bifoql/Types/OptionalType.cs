using System.Collections.Generic;

namespace Bifoql.Types
{
    internal class OptionalType : BifoqlType
    {
        public BifoqlType Type { get; }

        public OptionalType(BifoqlType type)
        {
            Type = type;
        }

        public override object ToObject()
        {
            return new { optionalOf = Type.ToObject() };
        }

        public override bool Equals(object other)
        {
            var otherOptional = other as OptionalType;
            if (otherOptional == null) return false;

            return Type.Equals(otherOptional.Type);
        }

        public override int GetHashCode()
        {
            return 209837492;
        }

        internal override string ToString(int indent)
        {
            return Type.ToString(indent) + "?";
        }
        internal override IEnumerable<NamedType> ReferencedNamedTypes => Type.ReferencedNamedTypes;
    }
}