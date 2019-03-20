using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncNull : AsyncObjectBase, IAsyncNull
    {
        public Task<bool> IsEqualTo(IAsyncObject other)
        {
            return Task.FromResult(other is IAsyncNull || other == null);
        }

        public override bool Equals(object other)
        {
            return other is IAsyncNull || other == null;
        }

        public override int GetHashCode()
        {
            return 283492837;
        }

        private AsyncNull()
        {
        }

        public Task<BifoqlType> GetSchema() => Task.FromResult<BifoqlType>(ScalarType.Null);


        public static readonly AsyncNull Instance = new AsyncNull();
    }
}
