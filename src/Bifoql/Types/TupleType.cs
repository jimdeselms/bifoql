namespace Bifoql.Types
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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

        internal override string ToString(int indent)
        {
            var builder = new StringBuilder();
            builder.AppendLine("[");

            for (int i = 0; i < Types.Length; i++)
            {
                string comma = i == Types.Length - 1 ? "" : ",";
                builder.AppendLine($"{Indent(indent+1)}{Types[i].ToString(indent+1)}{comma}");
            }

            builder.Append($"{Indent(indent)}]");

            return builder.ToString();
        }
        public override IEnumerable<NamedType> ReferencedNamedTypes => Types.SelectMany(t => t.ReferencedNamedTypes);

    }
}