using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Bifoql.Types
{
    using System.Collections.Generic;

    internal class MapType : BifoqlType
    {
        public IReadOnlyList<MapProperty> Properties { get; }

        public MapType(IReadOnlyList<MapProperty> properties)
        {
            Guard.ArgumentNotNull(properties, nameof(properties));
            Properties = properties;
        }

        public override object ToObject()
        {
            var result = new List<object>();

            foreach (var prop in Properties)
            {
                result.Add(new { name = prop.Name, value = prop.ToObject()});
            }

            return result;
        }

        public override bool IsCompound => true;

        public override int GetHashCode()
        {
            var code = 939283;

            foreach (var prop in Properties)
            {
                code ^= prop.GetHashCode();
            }

            return code;
        }

        internal override string GetDocumentation(int indent)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");

            int i = 0;
            foreach (var prop in Properties)
            {
                if (i > 0 && prop.Documentation != null)
                {
                    builder.AppendLine();
                }
                var comma = ++i == Properties.Count ? "" : ",";
                builder.AppendLine($"{prop.GetDocumentation(indent+1)}{comma}");
            }

            builder.Append($"{Indent(indent)}}}");

            return builder.ToString();
        }
    }
}