using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncUndefined : AsyncObjectBase, IBifoqlUndefined
    {
        public Task<bool> IsEqualTo(IBifoqlObject other)
        {
            return Task.FromResult(other is IBifoqlUndefined);
        }

        public override bool Equals(object other)
        {
            return other is IBifoqlNull;
        }

        public override int GetHashCode()
        {
            return 1144323323;
        }

        private AsyncUndefined()
        {
        }

        public static readonly AsyncUndefined Instance = new AsyncUndefined();
    }
}
