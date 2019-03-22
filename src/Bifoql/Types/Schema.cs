namespace Bifoql.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static class Schema
    {
        public static readonly BifoqlType Any = BifoqlType.Any;
        public static readonly BifoqlType Unknown = BifoqlType.Unknown;
        public static readonly BifoqlType Null = BifoqlType.Null;
        public static readonly BifoqlType Undefined = BifoqlType.Undefined;
        public static readonly BifoqlType Error = BifoqlType.Error;
        public static readonly BifoqlType Number = BifoqlType.Number;
        public static readonly BifoqlType String = BifoqlType.String;
        public static readonly BifoqlType Boolean = BifoqlType.Boolean;

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

        public static BifoqlType KeyValuePairOf(BifoqlType valueType)
        {
            return new KeyValuePairType(valueType);
        }

        public static BifoqlType Tuple(params BifoqlType[] types)
        {
            return new TupleType(types);
        }

        public static BifoqlType Map(params KeyValuePair<string, BifoqlType>[] pairs)
        {
            var dict = pairs.ToDictionary(p => p.Key, p => p.Value);
            return new MapType(dict);
        }

        public static KeyValuePair<string, BifoqlType> Pair(string key, BifoqlType value)
        {
            return new KeyValuePair<string, BifoqlType>(key, value);
        }

        public static BifoqlType Index(BifoqlType resultType, params IndexParameter[] parameters)
        {
            return new IndexedType(resultType, parameters);
        }

        public static BifoqlType Named(string name, BifoqlType definition)
        {
            return new NamedType(name, definition);
        }

        public static IndexParameter IndexParameter(string name, BifoqlType type, bool optional=false)
        {
            return new IndexParameter(name, type, optional);
        }
    }

}