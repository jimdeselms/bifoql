namespace Bifoql.Types
{
    public class ScalarType : BifoqlType
    {
        public string Type { get; }

        public ScalarType(string type)
        {
            Type = type;
        }

        public override object ToObject()
        {
            return Type;
        }

        public override bool Equals(object other)
        {
            var otherScalar = other as ScalarType;
            if (otherScalar == null) return false;

            return otherScalar.Type == Type;
        }

        public override int GetHashCode()
        {
            return 29384234 ^ Type.GetHashCode();
        }

        internal override string ToString(int indent)
        {
            return Type;
        }
    }
}