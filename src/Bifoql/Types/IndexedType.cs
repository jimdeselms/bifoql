using System.Collections.Generic;

namespace Bifoql.Types
{
    using System.Linq;
    public class IndexedType : BifoqlType
    {
        public BifoqlType ResultType { get; }
        public IReadOnlyList<Index> Indexes { get; }

        public IndexedType(BifoqlType resultType, IReadOnlyList<Index> indexes)
        {
            ResultType = resultType;
            Indexes = indexes;
        }

        public override object ToObject()
        {
            return new 
            { 
                indexes = Indexes.Select(i => i.ToObject()),
                resultType = ResultType.ToObject()
            };
        }

        public override bool Equals(object other)
        {
            var otherType = other as IndexedType;
            if (otherType == null) return false;

            if (Indexes.Count != otherType.Indexes.Count) return false;

            if (!ResultType.Equals(otherType.ResultType)) return false;

            for (int i = 0; i < Indexes.Count; i++)
            {
                if (!Indexes[i].Equals(otherType.Indexes[i])) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var code = 19234724;

            foreach (var index in Indexes)
            {
                code ^= index.GetHashCode();
            }

            return code;
        }
    }
}