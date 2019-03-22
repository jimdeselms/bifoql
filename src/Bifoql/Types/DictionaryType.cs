using System.Collections.Generic;

namespace Bifoql.Types
{
    internal class DictionaryType : BifoqlType
    {
        public BifoqlType ValueType { get; }

        public DictionaryType(BifoqlType valueType)
        {
            ValueType = valueType;
        }

        public override object ToObject()
        {
            return new { dictionaryOf = ValueType.ToObject() };
        }

        public override bool Equals(object other)
        {
            var otherDict = other as DictionaryType;
            if (otherDict == null) return false;

            return ValueType.Equals(otherDict.ValueType);
        }

        public override int GetHashCode()
        {
            return ValueType.GetHashCode() ^ 41425334;
        }

        internal override BifoqlType GetElementType(int index)
        {
            return ValueType;
        }

        internal override IEnumerable<NamedType> ReferencedNamedTypes => ValueType.ReferencedNamedTypes;


        internal override string ToString(int indent)
        {
            return $"string => {ValueType.ToString(indent)}[]";
        }
    }
}