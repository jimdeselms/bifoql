using System.Collections.Generic;
using System.Linq;
using Bifoql.Adapters;

namespace Bifoql
{
    internal class QueryContext
    {
        public IBifoqlObject QueryTarget { get; }

        public VariableScope Variables { get; }

        public QueryContext(IBifoqlObject target, VariableScope scope)
        {
            QueryTarget = target;
            Variables = scope;
        }

        public QueryContext ReplaceTarget(IBifoqlObject newTarget)
        {
            return new QueryContext(newTarget, Variables);
        }

        public static QueryContext Empty = new QueryContext(AsyncNull.Instance, VariableScope.Empty);

        public QueryContext AddVariable(string key, IBifoqlObject value)
        {
            return new QueryContext(QueryTarget, Variables.AddVariable(key, value));
        }
    }
}