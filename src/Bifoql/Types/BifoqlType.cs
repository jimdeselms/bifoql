using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bifoql.Types
{
    public abstract class BifoqlType
    {
        public static readonly BifoqlType Any = new ScalarType("any");
        public static readonly BifoqlType Unknown = new ScalarType("unknown");
        public static readonly BifoqlType Null = new ScalarType("null");
        public static readonly BifoqlType Undefined = new ScalarType("undefined");
        public static readonly BifoqlType Error = new ScalarType("error");
        public static readonly BifoqlType Number = new ScalarType("number");
        public static readonly BifoqlType String = new ScalarType("string");
        public static readonly BifoqlType Boolean = new ScalarType("boolean");

        public abstract object ToObject();

        // For container types, get the type of the nth element,
        // or "Unknown" if this isn't a container type
        internal virtual BifoqlType GetElementType(int index)
        {
            return BifoqlType.Unknown;
        }

        // For map types, get the type of the element at the given key
        // or "Unknown" if this isn't a map type.
        internal virtual BifoqlType GetKeyType(string key)
        {
            return BifoqlType.Unknown;
        }

        public virtual IEnumerable<NamedType> ReferencedNamedTypes => Enumerable.Empty<NamedType>();

        internal abstract string ToString(int indent);
        internal string Indent(int i)
        {
            return "".PadRight(i * 4);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(ToString(0));

            bool first = true;
            foreach (var type in ReferencedNamedTypes.Distinct().OrderBy(t => t.Name))
            {
                if (first)
                {
                    builder.AppendLine();
                    first = false;
                }
                builder.AppendLine();
                builder.AppendLine($"{type.Name} {type.Type.ToString(0)}");
            }

            return builder.ToString();
        }
    }
}