using System.Collections.Generic;

namespace Bifoql.Types
{
    using System.Collections.Generic;

    internal class KeyValuePairType : BifoqlType
    {
        public BifoqlType ValueType { get; }

        public KeyValuePairType(BifoqlType valueType)
        {
            ValueType = valueType;
        }

        public override object ToObject()
        {
            return new { keyValuePairOf = ValueType.ToObject() };
        }

        public override bool Equals(object other)
        {
            var otherKeyValuePair = other as KeyValuePairType;
            if (otherKeyValuePair == null) return false;

            return ValueType.Equals(otherKeyValuePair.ValueType);
        }

        public override int GetHashCode()
        {
            return ValueType.GetHashCode() ^ -83629234;
        }

        internal override IEnumerable<NamedType> ReferencedNamedTypes => ValueType.ReferencedNamedTypes;

        internal override string ToString(int indent)
        {
            return ValueType.ToString(indent);
        }
    }
}