namespace Bifoql.Types
{
    internal static class TypeCompatibilityChecker
    {
        public static bool IsCompatible(BifoqlType moreSpecific, BifoqlType lessSpecific)
        {
            if (moreSpecific.Equals(lessSpecific)) return true;

            if (moreSpecific is AnyType) return true;

            if (lessSpecific is OptionalType) return IsCompatibleWithOptional(moreSpecific, ((OptionalType)lessSpecific));
            if (lessSpecific is ArrayType) return IsCompatibleWithArray(moreSpecific, ((ArrayType)lessSpecific));
            if (lessSpecific is MapType) return IsCompatibleWithMap(moreSpecific, ((MapType)lessSpecific));
            if (lessSpecific is TupleType) return IsCompatibleWithTuple(moreSpecific, ((TupleType)lessSpecific));

            return false;
        }

        private static bool IsCompatibleWithOptional(BifoqlType moreSpecific, OptionalType optional)
        {
            if (moreSpecific.Equals(BifoqlType.Null)) return true;
            
            var moreSpecificOptional = moreSpecific as OptionalType;
            if (moreSpecificOptional != null) return IsCompatible(moreSpecificOptional.Type, optional.Type);

            return IsCompatible(moreSpecific, optional.Type);
        }

        private static bool IsCompatibleWithMap(BifoqlType moreSpecific, MapType map)
        {
            var moreSpecificMap = moreSpecific as MapType;
            if (moreSpecific != null)
            {
                // Go through all the keys in the map and make sure they're in the real one.
                foreach (var pair in map.Properties)
                {
                    var type = moreSpecificMap.GetKeyType(pair.Key);
                    if (!TypeCompatibilityChecker.IsCompatible(type, pair.Value))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        private static bool IsCompatibleWithArray(BifoqlType moreSpecific, ArrayType array)
        {
            var moreSpecificArray = moreSpecific as ArrayType;
            if (moreSpecificArray != null)
            {
                return IsCompatible(moreSpecificArray.ElementType, array.ElementType);
            }

            var moreSpecificTuple = moreSpecific as TupleType;
            if (moreSpecificTuple != null)
            {
                foreach (var tupleType in moreSpecificTuple.Types)
                {
                    if (!IsCompatible(tupleType, array.ElementType)) return false;
                }
                return true;
            }

            return false;
        }

        private static bool IsCompatibleWithTuple(BifoqlType moreSpecific, TupleType tuple)
        {
            var moreSpecificTuple = moreSpecific as TupleType;
            if (moreSpecificTuple != null)
            {
                if (tuple.Types.Length != moreSpecificTuple.Types.Length) return false;

                for (int i = 0; i < tuple.Types.Length; i++)
                {
                    if (!IsCompatible(moreSpecificTuple.Types[i], tuple.Types[i])) return false;
                }

                return true;
            }

            return false;
        }
    }
}