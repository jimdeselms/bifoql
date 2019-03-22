using System.Collections.Generic;

namespace Bifoql.Types
{
    internal class ArrayType : BifoqlType
    {
        public BifoqlType ElementType { get; }

        public ArrayType(BifoqlType elementType)
        {
            Guard.ArgumentNotNull(elementType, nameof(elementType));
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

        internal override string GetDocumentation(int indent)
        {
            return ElementType.GetDocumentation(indent) + "[]";
        }
    }
}