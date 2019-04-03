using System;
using System.Collections.Generic;
using System.Linq;
using Bifoql.Adapters;

namespace Bifoql
{
    internal class VariableScope
    {
        private VariableScope _previous;
        private string _name;
        private IBifoqlObject _value;

        internal static VariableScope Empty = new VariableScope(null, null, null);

        public VariableScope(VariableScope previous, string name, IBifoqlObject value)
        {
            _previous = previous == Empty ? null : previous;
            _name = name;
            _value = value;
        }

        public bool TryGetValue(string key, out IBifoqlObject result)
        {
            if (_name == key)
            {
                result = _value;
                return true;
            }
            
            if (_previous != null)
            {
                return _previous.TryGetValue(key, out result);
            }

            result = null;
            return false;
        }

        public bool ContainsKey(string key)
        {
            return _name == key || _previous?.ContainsKey(key) == true;
        }

        public VariableScope AddVariable(string key, IBifoqlObject value)
        {
            key = key.ToLower();
            if (ContainsKey(key))
            {
                throw new Exception($"Variable {key} already defined");
            }
            return new VariableScope(this, key, value);
        }
    }
}