using System.Collections.Generic;

namespace Bifoql.Types
{
    public class ArrayType : BifoqlType
    {
        public BifoqlType ElementType { get; }

        public ArrayType(BifoqlType elementType)
        {
            ElementType = elementType;
        }

        public override object ToObject()
        {
            return new { arrayOf = ElementType.ToObject() };
        }

        public override bool Equals(object other)
        {
            var otherArray = other as ArrayType;
            if (otherArray == null) return false;

            return ElementType.Equals(otherArray.ElementType);
        }

        public override int GetHashCode()
        {
            return ElementType.GetHashCode() ^ 98239234;
        }

        internal override BifoqlType GetElementType(int index)
        {
            return ElementType;
        }

        public override IEnumerable<NamedType> ReferencedNamedTypes => ElementType.ReferencedNamedTypes;


        internal override string ToString(int indent)
        {
            return ElementType.ToString(indent) + "[]";
        }
    }
}