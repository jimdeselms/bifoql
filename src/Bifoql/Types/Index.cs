using System.Collections.Generic;

namespace Bifoql.Types
{
    using System.Linq;

    public class Index
    {
        public IReadOnlyDictionary<string, BifoqlType> Keys { get; }

        public Index(IReadOnlyDictionary<string, BifoqlType> keys)
        {
            Keys = keys;
        }

        public object ToObject()
        {
            var keys = Keys.ToDictionary(p => p.Key, p => p.Value.ToObject());

            return new 
            {
                keys = keys,
            };
        }

        public override bool Equals(object other)
        {
            var otherIndex = other as Index;
            if (otherIndex == null) return false;
            if (Keys.Count != otherIndex.Keys.Count) return false;

            foreach (var pair in Keys)
            {
                BifoqlType otherType;
                if (!otherIndex.Keys.TryGetValue(pair.Key, out otherType))
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

        public override int GetHashCode()
        {
            var code = 298384724;

            foreach (var pair in Keys)
            {
                code ^= pair.Key.GetHashCode();
                code ^= pair.Value.GetHashCode();
            }

            return code;
        }
    }
}