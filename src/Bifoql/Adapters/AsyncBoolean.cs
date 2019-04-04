using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncBoolean : AsyncObjectBase, IBifoqlBoolean
    {
        public Task<bool> Value { get; }

        public AsyncBoolean(bool value)
        {
            Value = Task.FromResult(value);
        }

        public async Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) return true;
            
            var o = other as IBifoqlBoolean;
            if (o == null) return false;

            return (await o.Value) == await Value;
        }
    }
}