namespace Bifoql.Expressions
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using Bifoql.Adapters;

    internal class FilterExpr : Expr
    {
        public Expr Condition { get; }

        public FilterExpr(Expr condition) : base(condition.Location)
        {
            Condition = condition;
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            return new FilterExpr(Condition.Simplify(variables));
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            if (!Condition.NeedsAsync(context.Variables))
            {
                var value = Condition.Simplify(context.Variables).Apply(QueryContext.Empty).Result;
                // This isn't a filter; it's an array index
                if (value is IAsyncNumber)
                {
                    var theList = context.QueryTarget as IAsyncArray;
                    if (theList == null) return new AsyncError(this.Location, "Index must only be applied to an array");

                    var index = Convert.ToInt32(((IAsyncNumber)value).Value.Result);

                    if (index < 0)
                    {
                        index = theList.Count + index;
                    }

                    if (index >= 0 && index < theList.Count)
                    {
                        return await theList[index]();
                    }
                    else
                    {
                        return AsyncNull.Instance;
                    }
                }

                // This isn't a filter; it's a map key
                if (value is IAsyncString)
                {   
                    var theMap = context.QueryTarget as IAsyncMap;
                    if (theMap == null) return new AsyncError(this.Location, "Key lookup must only be applied to a map");

                    var key = ((IAsyncString)value).Value.Result;
                    return await theMap[key]();
                }
            }

            var list = context.QueryTarget as IAsyncArray;
            if (list == null) return new AsyncError(this.Location, "Can only apply filter to an array");

            var result = new List<Func<Task<IAsyncObject>>>();

            var resolvedCondition = Condition.Simplify(context.Variables);

            foreach (var item in list)
            {
                var val = await item();
                var condition = (await Condition.Apply(context.ReplaceTarget(val))) as IAsyncBoolean;
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
            return $"[{Condition.ToString()}]";
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables) => true;
    }
}