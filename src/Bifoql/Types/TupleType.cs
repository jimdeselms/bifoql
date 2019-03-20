namespace Bifoql.Types
{
    using System.Linq;

    public class TupleType : BifoqlType
    {
        public BifoqlType[] Types { get; }

        public TupleType(BifoqlType[] types)
        {
            Types = types;
        }

        public override object ToObject()
        {
            return Types.Select(t => t.ToObject()).ToArray();
        }

        internal override BifoqlType GetElementType(int i)
        {
            if (i >= 0 && i < Types.Length)
            {
                return Types[i];
            }
            else
            {
                return BifoqlType.Unknown;
            }
        }

        public override bool Equals(object other)
        {
            var otherTuple = other as TupleType;
            if (otherTuple == null) return false;
            if (otherTuple.Types.Length != Types.Length) return false;

            for (int i = 0; i < Types.Length; i++)
            {
                if (!otherTuple.Types[i].Equals(Types[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int code = -238234;

            foreach (var type in Types)
            {
                code ^= type.GetHashCode();
            }

            return code;
        }
    }
}