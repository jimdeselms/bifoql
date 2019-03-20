using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    internal class ArrayTypeInferer
    {
        public static BifoqlType InferArrayType(IReadOnlyList<BifoqlType> types)
        {
            var commonType = GetCommonType(types);
            if (commonType != null)
            {
                return Schema.ArrayOf(commonType);
            }
            else
            {
                return Schema.Tuple(types.ToArray());
            }
        }

        private static BifoqlType GetCommonType(IReadOnlyList<BifoqlType> types)
        {
            if (types.Count == 0) return null;

            var commonType = types[0];

            for (int i = 1; i < types.Count; i++)
            {
                commonType = GetCommonType(commonType, types[i]);
                if (commonType == null) break;
            }

            return commonType;
        }

        private static BifoqlType GetCommonType(BifoqlType t1, BifoqlType t2)
        {
            if (t1 == t2 || t1.Equals(t2))
            {
                return t1;
            }
            else
            {
                // Any is always compatible.
                if (t1.Equals(BifoqlType.Any) || t2.Equals(BifoqlType.Any)) return BifoqlType.Any;
                if (t1 is ArrayType && t2 is ArrayType) return GetCommonType((ArrayType)t1, (ArrayType)t2);
                if (t1 is OptionalType && t2 is OptionalType) return GetCommonType((OptionalType)t1, (OptionalType)t2);
                
                // If the second is null or optional, swap them to make the comparisons simpler.
                if (t2.Equals(BifoqlType.Null) || t2 is OptionalType || t2 is ArrayType)
                {
                    var swap = t2;
                    t2 = t1;
                    t1 = swap;
                }

                // If one of the types is null or optional, then get the optional version of the other one
                if (t1.Equals(BifoqlType.Null)) return Schema.Optional(t2);
                if (t1 is OptionalType) return GetCommonType((OptionalType)t1, t2);
                if (t1 is ArrayType && t2 is TupleType) return GetCommonType((ArrayType)t1, (TupleType)t2);

                var optional1 = t1 as OptionalType;

                return null;
            }
        }

        private static BifoqlType GetCommonType(ArrayType t1, ArrayType t2)
        {
            var common = GetCommonType(t1.ElementType, t2.ElementType);
            if (common == null) return null;

            return Schema.ArrayOf(common);
        }

        private static BifoqlType GetCommonType(ArrayType t1, TupleType t2)
        {
            foreach (var type in t2.Types)
            {
                if (!TypeCompatibilityChecker.IsCompatible(type, t1.ElementType))
                {
                    return null;
                }
            }

            return t1;
        }

        private static BifoqlType GetCommonType(OptionalType t1, OptionalType t2)
        {
            var common = GetCommonType(t1.Type, t2.Type);
            if (common == null) return null;

            return Schema.Optional(common);
        }

        private static BifoqlType GetCommonType(OptionalType t1, BifoqlType t2)
        {
            var common = GetCommonType(t1.Type, t2);
            if (common != null) return Schema.Optional(common);

            return null;
        }
    }
}