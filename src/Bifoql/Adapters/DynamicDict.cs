using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Bifoql.Adapters
{
    internal class DynamicDict : DynamicObject, IReadOnlyDictionary<string, object>
    {
        private readonly IReadOnlyDictionary<string, object> _values;

        public IEnumerable<string> Keys => _values.Keys;

        public IEnumerable<object> Values => _values.Values;

        public int Count => _values.Count;

        public object this[string key] => _values[key];

        public DynamicDict(IReadOnlyDictionary<string, object> values)
        {
            _values = values;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _values.TryGetValue(binder.Name, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public bool TryGetValue(string key, out object value) => _values.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
