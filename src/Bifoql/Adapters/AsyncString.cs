using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncString : AsyncObjectBase, IBifoqlString
    {
        public Task<string> Value { get; }

        public AsyncString(string value)
        {
            Value = Task.FromResult(value);
        }

        public async Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) return true;
            
            var o = other as IBifoqlString;
            if (o == null) return false;

            return (await o.Value) == await Value;
        }
    }
}