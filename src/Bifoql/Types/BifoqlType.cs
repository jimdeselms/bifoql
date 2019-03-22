using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bifoql.Types
{
    public abstract class BifoqlType
    {
        public static readonly BifoqlType Any = new ScalarType("any");
        internal static readonly BifoqlType Unknown = new ScalarType("unknown");
        public static readonly BifoqlType Null = new ScalarType("null");
        public static readonly BifoqlType Undefined = new ScalarType("undefined");
        public static readonly BifoqlType Error = new ScalarType("error");
        public static readonly BifoqlType Number = new ScalarType("number");
        public static readonly BifoqlType String = new ScalarType("string");
        public static readonly BifoqlType Boolean = new ScalarType("boolean");

       public static BifoqlType Optional(BifoqlType type)
        {
            // Already optional? Just return it.
            return (type is OptionalType) 
                ? type
                : new OptionalType(type);
        }

        public static BifoqlType ArrayOf(BifoqlType elementType)
        {
            return new ArrayType(elementType);
        }

        public static BifoqlType DictionaryOf(BifoqlType valueType)
        {
            return new DictionaryType(valueType);
        }

        public static BifoqlType Tuple(params BifoqlType[] types)
        {
            return new TupleType(types);
        }

        public static BifoqlType Union(params BifoqlType[] types)
        {
            return new UnionType(types);
        }

        public static BifoqlType Map(params MapProperty[] pairs)
        {
            var dict = pairs.ToDictionary(p => p.Name, p => p);
            return new MapType(dict);
        }

        public static MapProperty Property(string key, BifoqlType value, string documentation=null)
        {
            return new MapProperty(key, value, documentation);
        }

        public static BifoqlType Index(BifoqlType resultType, params IndexParameter[] parameters)
        {
            return new IndexedType(resultType, parameters);
        }

        public static BifoqlType Named(string name)
        {
            return new NamedTypeReference(name);
        }

        public static BifoqlNamedType CreateNamedType(string name, BifoqlType type, string documentation = null)
        {
            return new BifoqlNamedType(name, type, documentation);
        }

        public static IndexParameter IndexParameter(string name, BifoqlType type, bool optional=false)
        {
            return new IndexParameter(name, type, optional);
        }
 
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

        internal abstract string GetDocumentation(int indent);
        internal static string Indent(int i)
        {
            return "".PadRight(i * 4);
        }

        internal static string FormatDocumentation(string documentation, int indent)
        {
            if (documentation != null)
            {
                var lines = documentation.Replace("\r", "").Split('\n');
                var text = new StringBuilder();
                foreach (var line in lines)
                {
                    var formatted = $"{Indent(indent)}// {line}";
                    text.AppendLine(formatted);
                }
                return text.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}