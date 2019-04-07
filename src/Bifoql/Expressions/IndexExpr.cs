namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

    internal class IndexExpr : Expr
    {
        private readonly Location _location;
        internal readonly Expr Target;
        private readonly Expr _index;

        public IndexExpr(Location location, Expr target, Expr index) : base(location)
        {
            _location = location;
            Target = target;
            _index = index;
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            Target?.Accept(visitor);
            _index.Accept(visitor);
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var target = Target == null
                ? context.QueryTarget
                : await Target.Apply(context, resolveDeferred: false);

            target = await target.GetDefaultValueFromIndex();

            // Propagate errors
            if (target is IBifoqlError) return target;

            var index = await _index.Apply(context);

            var lookup = target as IBifoqlMapInternal;
            if (lookup != null && index is IBifoqlString)
            {
                var key = await ((IBifoqlString)index).Value;
                Func<Task<IBifoqlObject>> value;
                if (lookup.TryGetValue(key, out value))
                {
                    return await value();
                }
                else
                {
                    return AsyncUndefined.Instance;
                }
            }

            var array = target as IBifoqlArrayInternal;
            if (array != null && index is IBifoqlNumber)
            {
                var i = (int)await ((IBifoqlNumber)index).Value;
                if (i < 0)
                {
                    if (-i > array.Count)
                    {
                        return AsyncUndefined.Instance;
                    }

                    return await array[array.Count + i]();
                }
                else
                {
                    if (i >= array.Count)
                    {
                        return AsyncUndefined.Instance;
                    }

                    return await array[i]();
                }
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

            return $"{target}[{RightHandSideString()}]";
        }
        
        private string RightHandSideString()
        {
            return $"[{_index.ToString()}]";
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new IndexExpr(
                _location, 
                Target?.Simplify(variables),
                _index.Simplify(variables));
        }
    }
}