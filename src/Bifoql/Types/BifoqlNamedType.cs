using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    public class BifoqlNamedType : BifoqlType
    {
        public string Name { get; }
        public BifoqlType Type { get; }
        private readonly string _documentation;

        public BifoqlNamedType(string name, BifoqlType type, string documentation=null)
        {
            Guard.ArgumentNotNull(name, nameof(name));
            Guard.ArgumentNotNull(type, nameof(type));
            Name = name;
            Type = type;
            _documentation = documentation;
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

        internal override string GetDocumentation(int indent)
        {
            return $"{FormatDocumentation(_documentation, indent)}{Indent(indent)}{Name} = {Type.GetDocumentation(indent)}";
        }
    }
}