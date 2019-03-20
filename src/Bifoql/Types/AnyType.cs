namespace Bifoql.Types
{
    internal class AnyType : ScalarType
    {
        public AnyType() : base("any")
        {
        }

        internal override BifoqlType GetElementType(int index)
        {
            return this;
        }

        internal override BifoqlType GetKeyType(string key)
        {
            return this;
        }
    }
}