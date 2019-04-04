namespace Bifoql.Expressions
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using Bifoql.Adapters;
    using Bifoql.Extensions;

    internal class FilterExpr : Expr
    {
        public Expr Target { get; }
        public Expr Condition { get; }

        public FilterExpr(Expr target, Expr condition) : base(condition.Location)
        {
            Target = target;
            Condition = condition;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new FilterExpr(
                Target?.Simplify(variables),
                Condition.Simplify(variables));
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var target = Target == null
                ? context.QueryTarget
                : await Target.Apply(context, resolveDeferred: false);

            target = await target.GetDefaultValueFromIndex();

            // Propagate errors.
            if (target is IBifoqlError) return target;

            var deferred = target as IBifoqlDeferredQueryInternal;
            if (deferred != null)
            {   
                return DeferredQueryWrapper.AddToQuery(deferred, RightHandSideString());
            }

            var list = target as IBifoqlArrayInternal;
            if (list == null) return new AsyncError(this.Location, "Can only apply filter to an array");

            var result = new List<Func<Task<IBifoqlObject>>>();

            var resolvedCondition = Condition.Simplify(context.Variables);

            foreach (var item in list)
            {
                var val = await item();
                var condition = (await Condition.Apply(context.ReplaceTarget(val))) as IBifoqlBoolean;
                if (condition == null)
                {
                    return new AsyncError(this.Location, "Filter condition must evaluate to boolean");
                }

                if (await condition.Value)
                {
                    result.Add(item);
                }
            }

            return new AsyncArray(result);
        }

        public override string ToString()
        {
            var target = Target == null
                ? ""
                : Target.ToString();

            return $"{Target}{RightHandSideString()}";
        }

        private string RightHandSideString()
        {
            return $"[? {Condition.ToString()}]";
        }

        public override bool NeedsAsync(VariableScope variables) => true;
        public override bool ReferencesRootVariable => Condition.ReferencesRootVariable;

    }
}