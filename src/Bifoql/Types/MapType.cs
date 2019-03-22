using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Bifoql.Types
{
    using System.Collections.Generic;

    internal class MapType : BifoqlType
    {
        public IReadOnlyDictionary<string, BifoqlType> Properties { get; }

        public MapType(IReadOnlyDictionary<string, BifoqlType> properties)
        {
            Properties = properties;
        }

        public override object ToObject()
        {
            var dict = new Dictionary<string, object>();

            foreach (var prop in Properties)
            {
                dict[prop.Key] = prop.Value.ToObject();
            }

            return dict;
        }

        public override bool Equals(object other)
        {
            var otherMap = other as MapType;
            if (otherMap == null) return false;
            if (Properties.Count != otherMap.Properties.Count) return false;

            foreach (var pair in Properties)
            {
                BifoqlType otherType;
                if (!otherMap.Properties.TryGetValue(pair.Key, out otherType))
                {
                    return false;
                }

                if (!pair.Value.Equals(otherType))
                {
                    return false;
                }
            }

            return true;
        }

        internal override BifoqlType GetKeyType(string key)
        {
            BifoqlType type;
            if (Properties.TryGetValue(key, out type))
            {
                return type;
            }
            else
            {
                return BifoqlType.Null;
            }
        }

        public override int GetHashCode()
        {
            var code = 939283;

            foreach (var pair in Properties)
            {
                code ^= pair.Key.GetHashCode();
                code ^= pair.Value.GetHashCode();
            }

            return code;
        }

        internal override string ToString(int indent)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");

            int i = 0;
            foreach (var prop in Properties)
            {
                var comma = ++i == Properties.Count ? "" : ",";
                builder.AppendLine($"{Indent(indent+1)}{prop.Key}: {prop.Value.ToString(indent+1)}{comma}");
            }

            builder.Append($"{Indent(indent)}}}");

            return builder.ToString();
        }

        internal override IEnumerable<NamedType> ReferencedNamedTypes => Properties.Values.SelectMany(p => p.ReferencedNamedTypes);
    }
}