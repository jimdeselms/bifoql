namespace Bifoql.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static class Schema
    {

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

        public static BifoqlType Indexed(BifoqlType resultType, params Index[] indexes)
        {
            return new IndexedType(resultType, indexes);
        }

        public static Index Index(params KeyValuePair<string, BifoqlType>[] keys)
        {
            var dict = keys.ToDictionary(k => k.Key, k => k.Value);
            return new Index(dict);
        }
    }

}