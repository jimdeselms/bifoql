namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

    internal class KeyExpr : Expr
    {
        private readonly Location _location;
        internal readonly Expr Target;
        private readonly string _key;

        public KeyExpr(Location location, Expr target, string key) : base(location)
        {
            _location = location;
            Target = target;
            _key = key;
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var target = Target == null
                ? context.QueryTarget
                : await Target.Apply(context, resolveDeferred: false);

            target = await target.GetDefaultValueFromIndex();

            return await GetKeyFromObject(target, context);
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            Target?.Accept(visitor);
        }


        private async Task<IBifoqlObject> GetKeyFromObject(IBifoqlObject target, QueryContext context)
        {
            // Propagate errors.
            if (target is IBifoqlError) return target;

            var lookup = target as IBifoqlLookupInternal;
            if (lookup != null)
            {
                Func<Task<IBifoqlObject>> value;
                if (lookup.TryGetValue(_key, out value))
                {
                    return await value();
                }
                else
                {
                    return new AsyncError(Location, $"key '{_key}' not found");
                }
            }

            var array = target as IBifoqlArrayInternal;
            if (array != null)
            {
                var result = new List<Func<Task<IBifoqlObject>>>();

                foreach (var item in array)
                {
                    var resolvedItem = await item();
                    var entry = GetKeyFromObject(resolvedItem, context);
                    result.Add(() => entry);
                }

                return new AsyncArray(result);
            }

            if (target is IBifoqlUndefined)
            {
                return AsyncUndefined.Instance;
            }

            var deferred = target as IBifoqlDeferredQueryInternal;
            if (deferred != null)
            {
                return DeferredQueryWrapper.AddToQuery(deferred, RightHandSideString());
            }

            return new AsyncError(this.Location, "key expression must be applied to array or map");
        }

        public override string ToString()
        {
            var target = Target == null
                ? ""
                : $"{Target.ToString()}";

            var rhs = RightHandSideString();

            return $"{target}{rhs}";
        }

        private string RightHandSideString()
        {
            bool isEscaped;
            var key = Utilities.Escape(_key, out isEscaped);

            var dot = isEscaped ? "" : ".";

            return isEscaped ? $"[\"{key}\"]" : $"{dot}{key}";
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new KeyExpr(_location, Target?.Simplify(variables), _key);
        }
    }
}