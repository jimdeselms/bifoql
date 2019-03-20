using System.Collections.Generic;
using System.Linq;
using Bifoql.Adapters;

namespace Bifoql
{
    public class QueryContext
    {
        public IBifoqlObject QueryTarget { get; }
        public IReadOnlyDictionary<string, IBifoqlObject> Variables { get; }

        public QueryContext(IBifoqlObject target, IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            QueryTarget = target;
            Variables = variables;
        }

        public QueryContext ReplaceTarget(IBifoqlObject newTarget)
        {
            return new QueryContext(newTarget, Variables);
        }

        public static QueryContext Empty = new QueryContext(AsyncNull.Instance, new Dictionary<string, IBifoqlObject>());

        public QueryContext AddVariable(string key, IBifoqlObject value)
        {
            var newDict = Variables.ToDictionary(p => p.Key, p => p.Value);
            newDict[key] = value;

            return new QueryContext(QueryTarget, newDict);
        }
    }
}