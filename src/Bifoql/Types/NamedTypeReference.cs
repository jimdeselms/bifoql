using System;
using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    internal class NamedTypeReference : BifoqlType
    {
        public Func<BifoqlNamedType> Reference { get; }
        public NamedTypeReference(Func<BifoqlNamedType> reference)
        {
            Guard.ArgumentNotNull(reference, nameof(reference));
            Reference = reference;
        }

        public override object ToObject()
        {
            return Reference().Name;
        }

        public override bool Equals(object other)
        {
            var otherRef = other as NamedTypeReference;
            return otherRef != null && otherRef.Reference().Name == Reference().Name;
        }

        public override int GetHashCode()
        {
            return 2983423 ^ Reference().Name.GetHashCode();
        }

        internal override string GetDocumentation(int indent)
        {
            return Reference().Name;
        }
    }
}