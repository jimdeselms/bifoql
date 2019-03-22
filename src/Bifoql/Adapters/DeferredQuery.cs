using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class DeferredQuery : IBifoqlDeferredQueryInternal
    {
        private readonly IBifoqlDeferredQuery _query;

        public DeferredQuery(IBifoqlDeferredQuery query)
        {
            _query = query;
        }

        public Task<object> EvaluateQuery(string query)
        {
            return _query.EvaluateQuery(query);
        }

        public Task<bool> IsEqualTo(IBifoqlObject o)
        {
            return Task.FromResult(this == o);
        }
    }
}
