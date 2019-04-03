using System;
using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncIndex : AsyncObjectWithDefaultValueBase, IBifoqlIndexInternal
    {
        private readonly Func<IndexArgumentList, Task<object>> _lookup;
        private readonly BifoqlType _schema;

        public AsyncIndex(Func<IndexArgumentList, Task<object>> lookup, 
            Func<Task<IBifoqlObject>> defaultValue,
            BifoqlType schema=null) : base(defaultValue)
        {
            _lookup = lookup;
            _schema = schema;
        }

        public Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) return Task.FromResult(true);
            
            var o = other as AsyncIndex;
            return Task.FromResult(o?._lookup == this._lookup);
        }
        
        public Task<object> Lookup(IndexArgumentList arguments)
        {
            return _lookup(arguments);
        }
    }
}