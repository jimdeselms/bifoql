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
            Guard.ArgumentNotNull(name, nameof(name));
            Guard.ArgumentNotNull(type, nameof(type));

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

        internal string ToString(int indent)
        {
            var optionalOperator = Optional ? "?" : "";
            return $"{Name}{optionalOperator}: {Type.GetDocumentation(indent)}";
        }
    }
}