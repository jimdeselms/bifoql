namespace Bifoql.Types
{
    public abstract class BifoqlType
    {
        public static readonly BifoqlType Any = new AnyType();
        public static readonly BifoqlType Unknown = new ScalarType("unknown");
        public static readonly BifoqlType Null = new ScalarType("null");
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
    }
}