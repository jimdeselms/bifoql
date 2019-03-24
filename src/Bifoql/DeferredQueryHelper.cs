using System.Threading.Tasks;
using Bifoql.Extensions;

namespace Bifoql
{
    internal class DeferredQueryWrapper : IBifoqlDeferredQueryInternal
    {
        private readonly IBifoqlDeferredQueryInternal _obj;
        private readonly string _query;

        public static DeferredQueryWrapper AddToQuery(IBifoqlDeferredQueryInternal obj, string query)
        {
            var deferred = obj as DeferredQueryWrapper;
            if (deferred != null)
            {
                return new DeferredQueryWrapper(deferred._obj, deferred._query + query);
            }
            else
            {
                return new DeferredQueryWrapper(obj, "@" + query);
            }
        }

        public static async Task<IBifoqlObject> EvaluateDeferredQuery(IBifoqlObject bifoqlObject)
        {
            var deferred = bifoqlObject as DeferredQueryWrapper;
            if (deferred != null)
            {
                return (await deferred._obj.EvaluateQuery(deferred._query)).ToBifoqlObject();
            }            
            else 
            {
                return bifoqlObject;
            }
        }

        public Task<object> EvaluateQuery(string query)
        {
            return _obj.EvaluateQuery(query);
        }

        public Task<bool> IsEqualTo(IBifoqlObject o)
        {
            var other = o as DeferredQueryWrapper;
            return Task.FromResult(o != null && _obj == other._obj && _query == other._query);
        }

        private DeferredQueryWrapper(IBifoqlDeferredQueryInternal obj, string query)
        {
            _obj = obj;
            _query = query;
        }
    }
}