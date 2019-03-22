namespace Bifoql.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class UnionType : BifoqlType
    {
        public BifoqlType[] Types { get; }

        public UnionType(BifoqlType[] types)
        {
            Guard.ArgumentNotNull(types, nameof(types));

            foreach (var type in types)
            {
                if (type == null)
                {
                    throw new ArgumentException($"types must not have any empty entries");
                }
            }
            Types = types;
        }

        public override object ToObject()
        {
            return new {
                unionOf = Types.Select(t => t.ToObject()).ToArray()
            };
        }

        public override bool Equals(object other)
        {
            var otherUnion = other as UnionType;
            if (otherUnion == null) return false;
            if (otherUnion.Types.Length != Types.Length) return false;

            for (int i = 0; i < Types.Length; i++)
            {
                if (!otherUnion.Types[i].Equals(Types[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int code = -78098230;

            foreach (var type in Types)
            {
                code ^= type.GetHashCode();
            }

            return code;
        }

        internal override string ToString(int indent)
        {
            return string.Join(" | ", Types.Select(t => t.ToString(indent)));
        }
        internal override IEnumerable<NamedType> ReferencedNamedTypes => Types.SelectMany(t => t.ReferencedNamedTypes);

    }
}