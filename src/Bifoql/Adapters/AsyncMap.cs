using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bifoql.Types;
using Bifoql.Extensions;

namespace Bifoql.Adapters
{
    internal class AsyncMap : AsyncObjectBase, IAsyncMap
    {
        private readonly BifoqlType _type;

        private readonly IReadOnlyDictionary<string, Func<Task<IAsyncObject>>> _getters;

        public IEnumerable<string> Keys => _getters.Keys;

        public IEnumerable<Func<Task<IAsyncObject>>> Values => _getters.Values;

        public int Count => _getters.Count;

        public Func<Task<IAsyncObject>> this[string key] => _getters[key];

        public AsyncMap(IReadOnlyDictionary<string, Func<Task<IAsyncObject>>> getters, BifoqlType type=null)
        {
            _getters = getters;
            _type = type;
        }

        public bool ContainsKey(string key) => _getters.ContainsKey(key);

        public bool TryGetValue(string key, out Func<Task<IAsyncObject>> value) => _getters.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, Func<Task<IAsyncObject>>>> GetEnumerator() => _getters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task<bool> IsEqualTo(IAsyncObject other)
        {
            if (this == other) return true;
            
            var otherDict
             = other as IAsyncMap;
            if (otherDict == null) return false;
            if (otherDict.Count != this.Count) return false;

            // First go through the things that don't needs async; they'll be fast and cheap
            foreach (var key in this.Keys)
            {
                var unresolvedThis = this[key];
                Func<Task<IAsyncObject>> unresolvedThat;
                if (!otherDict.TryGetValue(key, out unresolvedThat)) return false;

                if (unresolvedThis == unresolvedThat) continue;

                var thisObj = await unresolvedThis();
                var thatObj = await unresolvedThat();

                if (!(await thisObj.IsEqualTo(thatObj)))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<BifoqlType> GetSchema()
        {
            if (_type != null) return _type;

            var dict = new Dictionary<string, BifoqlType>();

            foreach (var pair in _getters)
            {
                var val = await pair.Value();
                dict[pair.Key] = await val.GetSchema();
            }

            return new MapType(dict);
        }
    }
}
