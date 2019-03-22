using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    internal class NamedTypeReference : BifoqlType
    {
        public string Name { get; }
        public NamedTypeReference(string name)
        {
            Guard.ArgumentNotNull(name, nameof(name));
            Name = name;
        }

        public override object ToObject()
        {
            return Name;
        }

        public override bool Equals(object other)
        {
            var otherRef = other as NamedTypeReference;
            return otherRef?.Name == Name;
        }

        public override int GetHashCode()
        {
            return 2983423 ^ Name.GetHashCode();
        }

        internal override string GetDocumentation(int indent)
        {
            return Name;
        }
    }
}