using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    public class NamedType : BifoqlType
    {
        public string Name { get; }
        public BifoqlType Type { get; }

        public NamedType(string name, BifoqlType type)
        {
            Name = name;
            Type = type;
        }

        public override object ToObject()
        {
            return Name;
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

        public override IEnumerable<NamedType> ReferencedNamedTypes => Type.ReferencedNamedTypes.Concat(new [] { this });

        internal override string ToString(int indent)
        {
            return Name;
        }
    }
}