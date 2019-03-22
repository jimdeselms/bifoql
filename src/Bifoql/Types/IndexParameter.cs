using System.Collections.Generic;

namespace Bifoql.Types
{
    public class IndexParameter
    {
        public string Name { get; }
        public BifoqlType Type { get; }
        public bool Optional { get; }

        public IndexParameter(string name, BifoqlType type, bool optional)
        {
            Name = name;
            Type = type;
            Optional = optional;
        }

        public object ToObject()
        {
            return new {
                name = Name,
                type = Type.ToObject(),
                optional = Optional
            };
        }

        internal IEnumerable<NamedType> ReferencedNamedTypes => Type.ReferencedNamedTypes;

        internal string ToString(int indent)
        {
            var optionalOperator = Optional ? "?" : "";
            return $"{Name}{optionalOperator}: {Type.ToString(indent)}";
        }
    }
}