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
    internal class AsyncArray : IBifoqlArray
    {
        private readonly BifoqlType _type;
        private readonly IReadOnlyList<Func<Task<IBifoqlObject>>> _getters;

        public int Count => _getters.Count;

        public async Task<BifoqlType> GetSchema()
        {
            if (_type != null) return _type;

            var elementTypes = new List<BifoqlType>();

            foreach (var el in _getters)
            {
                var val = await el();
                elementTypes.Add(await val.GetSchema());
            }

            return ArrayTypeInferer.InferArrayType(elementTypes);
        }

        public Func<Task<IBifoqlObject>> this[int key] => _getters[key];

        public AsyncArray(IReadOnlyList<Func<Task<IBifoqlObject>>> getters, BifoqlType type=null)
        {
            _getters = getters;
            _type = type;
        }

        public Task<object> InvokeIndex(IndexArgumentList filter)
        {
            return Task.FromResult<object>(new AsyncError("This object has no indexes"));
        }

        public IEnumerator<Func<Task<IBifoqlObject>>> GetEnumerator() => _getters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public async Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) return true;
            
            var otherArray = other as IBifoqlArray;
            if (otherArray == null) return false;
            if (otherArray.Count != this.Count) return false;

            // First go through the things that don't needs async; they'll be fast and cheap
            for (var i = 0; i < Count; i++)
            {
                var unresolvedThis = this[i];
                var unresolvedThat = otherArray[i];

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
