namespace Bifoql.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class MapProperty : BifoqlType
    {
        public string Name { get; }
        public BifoqlType Type { get; }
        public string Documentation { get; }

        public MapProperty(string name, BifoqlType type, string documentation)
        {
            Guard.ArgumentNotNull(name, nameof(name));
            Guard.ArgumentNotNull(type, nameof(type));

            Name = name;
            Type = type;
            Documentation = documentation;
        }

        public override object ToObject()
        {
            return new KeyValuePair<string, object>(Name, Type.ToObject());
        }

        public override bool Equals(object other)
        {
            var otherProperty = other as MapProperty;
            if (otherProperty != null)
            {
                return otherProperty.Name == Name && otherProperty.Type.Equals(Type);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 33347234 ^ Name.GetHashCode() & Type.GetHashCode();
        }

        public override bool IsCompound => true;
        
        internal override string GetDocumentation(int indent)
        {
            return $"{FormatDocumentation(Documentation, indent)}{Indent(indent)}{Name}: {Type.GetDocumentation(indent)}";
        }
    }
}