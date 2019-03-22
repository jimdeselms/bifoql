using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncNull : AsyncObjectBase, IBifoqlNull
    {
        public Task<bool> IsEqualTo(IBifoqlObject other)
        {
            return Task.FromResult(other is IBifoqlNull || other == null);
        }

        public override bool Equals(object other)
        {
            return other is IBifoqlNull || other == null;
        }

        public override int GetHashCode()
        {
            return 283492837;
        }

        private AsyncNull()
        {
        }

        public static readonly AsyncNull Instance = new AsyncNull();
    }
}
