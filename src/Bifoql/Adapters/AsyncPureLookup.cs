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
    internal class AsyncPureLookup : AsyncObjectWithDefaultValueBase, IBifoqlLookupInternal, IBifoqlLookup
    {
        private IBifoqlLookup _lookup;

        public AsyncPureLookup(IBifoqlLookup lookup, Func<Task<IBifoqlObject>> defaultValue)
            : base(defaultValue)
        {
            _lookup = lookup;
        }

        public Func<Task<IBifoqlObject>> this[string key] 
        {
            get
            {
                Func<Task<IBifoqlObject>> result;
                if (TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    throw new KeyNotFoundException(key);
                }
            }
        }

        public bool ContainsKey(string key)
        {
            Func<Task<object>> result;
            return _lookup.TryGetValue(key, out result);
        }

        public Task<bool> IsEqualTo(IBifoqlObject o)
        {
            throw new Exception("Can't check equality on a lookup");
        }

        public bool TryGetValue(string key, out Func<Task<IBifoqlObject>> value)
        {
            Func<Task<object>> result;
            if (_lookup.TryGetValue(key, out result))
            {
                Task<object> resultTask = result();
                var toBifoql = resultTask.ContinueWith(o => o.Result.ToBifoqlObject());
                value = () => toBifoql;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool TryGetValue(string key, out Func<Task<object>> result)
        {
            return _lookup.TryGetValue(key, out result);
        }
    }

    internal class SyncPureLookup : AsyncObjectWithDefaultValueBase, IBifoqlLookupInternal, IBifoqlLookupSync
    {
        private IBifoqlLookupSync _lookup;

        public SyncPureLookup(IBifoqlLookupSync lookup, Func<Task<IBifoqlObject>> defaultValue)
            : base(defaultValue)
        {
            _lookup = lookup;
        }

        public Func<Task<IBifoqlObject>> this[string key] 
        {
            get
            {
                Func<Task<IBifoqlObject>> result;
                if (TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    throw new KeyNotFoundException(key);
                }
            }
        }

        public bool ContainsKey(string key)
        {
            Func<object> result;
            return _lookup.TryGetValue(key, out result);
        }

        public Task<bool> IsEqualTo(IBifoqlObject o)
        {
            throw new Exception("Can't check equality on a lookup");
        }

        public bool TryGetValue(string key, out Func<Task<IBifoqlObject>> value)
        {
            Func<object> result;
            if (_lookup.TryGetValue(key, out result))
            {
                value = () => Task.FromResult(result().ToBifoqlObject());
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool TryGetValue(string key, out Func<object> result)
        {
            return _lookup.TryGetValue(key, out result);
        }
    }

    internal class AsyncLookup : AsyncObjectWithDefaultValueBase, IBifoqlLookupInternal//IBifoqlMapInternal
    {
        private readonly BifoqlType _type;

        private readonly IReadOnlyDictionary<string, Func<Task<IBifoqlObject>>> _getters;

        public IEnumerable<string> Keys => _getters.Keys;

        public IEnumerable<Func<Task<IBifoqlObject>>> Values => _getters.Values;

        public int Count => _getters.Count;

        public Func<Task<IBifoqlObject>> this[string key] => _getters[key];

        public AsyncLookup(IReadOnlyDictionary<string, Func<Task<IBifoqlObject>>> getters, 
            Func<Task<IBifoqlObject>> defaultValue,
            BifoqlType type=null) : base(defaultValue)
        {
            _getters = getters;
            _type = type;
        }

        public bool ContainsKey(string key) => _getters.ContainsKey(key);

        public bool TryGetValue(string key, out Func<Task<IBifoqlObject>> value) => _getters.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, Func<Task<IBifoqlObject>>>> GetEnumerator() => _getters.GetEnumerator();

        // IEnumerator IEnumerable.GetEnumerator()
        // {
        //     return GetEnumerator();
        // }

        public async Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) return true;
            
            var otherDict = other as IBifoqlMapInternal;
            if (otherDict == null) return false;
            if (otherDict.Count != this.Count) return false;

            // First go through the things that don't needs async; they'll be fast and cheap
            foreach (var key in this.Keys)
            {
                var unresolvedThis = this[key];
                Func<Task<IBifoqlObject>> unresolvedThat;
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
    }
}
