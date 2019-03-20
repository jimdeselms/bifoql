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

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            var lookup = context.QueryTarget as IAsyncMap;
            if (lookup != null)
            {
                Func<Task<IAsyncObject>> value;
                if (lookup.TryGetValue(Key, out value))
                {
                    return await value();
                }
                else
                {
                    return AsyncNull.Instance;
                }
            }

            var array = context.QueryTarget as IAsyncArray;
            if (array != null)
            {
                var result = new List<Func<Task<IAsyncObject>>>();

                foreach (var item in array)
                {
                    var resolvedItem = await item();
                    var newContext = context.ReplaceTarget(resolvedItem);
                    var resolved = this.Apply(newContext);
                    result.Add(() => resolved);
                }

                return new AsyncArray(result);
            }

            if (context.QueryTarget is IAsyncNull)
            {
                return AsyncNull.Instance;
            }

            return new AsyncError(this.Location, "key expression must be applied to array or map");
        }

        public override string ToString()
        {
            bool isEscaped;
            var key = Utilities.Escape(Key, out isEscaped);

            return isEscaped ? $"[\"{key}\"]" : key;
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            // this can't be simplified.
            return this;
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables) => true;
        public override bool NeedsAsyncByItself => true;
    }
}