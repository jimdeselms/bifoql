namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class KeyExpr : Expr
    {
        public string Key;

        public KeyExpr(Location location, string key) : base(location)
        {
            Key = key;
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var lookup = context.QueryTarget as IBifoqlMapInternal;
            if (lookup != null)
            {
                Func<Task<IBifoqlObject>> value;
                if (lookup.TryGetValue(Key, out value))
                {
                    return await value();
                }
                else
                {
                    return AsyncUndefined.Instance;
                }
            }

            var array = context.QueryTarget as IBifoqlArrayInternal;
            if (array != null)
            {
                var result = new List<Func<Task<IBifoqlObject>>>();

                foreach (var item in array)
                {
                    var resolvedItem = await item();
                    var newContext = context.ReplaceTarget(resolvedItem);
                    var resolved = this.Apply(newContext);
                    result.Add(() => resolved);
                }

                return new AsyncArray(result);
            }

            if (context.QueryTarget is IBifoqlUndefined)
            {
                return AsyncUndefined.Instance;
            }

            return new AsyncError(this.Location, "key expression must be applied to array or map");
        }

        public override string ToString()
        {
            bool isEscaped;
            var key = Utilities.Escape(Key, out isEscaped);

            return isEscaped ? $"[\"{key}\"]" : key;
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            // this can't be simplified.
            return this;
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => true;
        public override bool NeedsAsyncByItself => true;
        public override bool ReferencesRootVariable => false;

    }
}