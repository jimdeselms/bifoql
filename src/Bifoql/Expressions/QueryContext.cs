using System.Collections.Generic;
using System.Linq;
using Bifoql.Adapters;

namespace Bifoql
{
    public class QueryContext
    {
        public IAsyncObject QueryTarget { get; }
        public IReadOnlyDictionary<string, IAsyncObject> Variables { get; }

        public QueryContext(IAsyncObject target, IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            QueryTarget = target;
            Variables = variables;
        }

        public QueryContext ReplaceTarget(IAsyncObject newTarget)
        {
            return new QueryContext(newTarget, Variables);
        }

        public static QueryContext Empty = new QueryContext(AsyncNull.Instance, new Dictionary<string, IAsyncObject>());

        public QueryContext AddVariable(string key, IAsyncObject value)
        {
            var newDict = Variables.ToDictionary(p => p.Key, p => p.Value);
            newDict[key] = value;

            return new QueryContext(QueryTarget, newDict);
        }
    }
}