namespace Bifoql.Expressions
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Bifoql.Adapters;

    internal class SliceExpr : Expr
    {
        private readonly Expr _lowerBound;
        private readonly Expr _upperBound;

        public SliceExpr(Location location, Expr lowerBound, Expr upperBound) : base(location)
        {
            // Using the location for these expressions is wrong, but who cares.
            _lowerBound = lowerBound ?? new LiteralExpr(location, AsyncNull.Instance);
            _upperBound = upperBound ?? new LiteralExpr(location, AsyncNull.Instance);
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            var list = context.QueryTarget as IAsyncArray;
            if (list == null) return new AsyncError(this.Location, "Can't take slice of non-list");

            var result = new List<Func<Task<IAsyncObject>>>();

            var lowerBoundObj = await _lowerBound.Apply(context);
            int? lowerBound = null;
            if (lowerBoundObj is IAsyncNumber)
            {
                lowerBound = (int)await ((IAsyncNumber)lowerBoundObj).Value;
            }

            var upperBoundObj = await _upperBound.Apply(context);
            int? upperBound = null;
            if (upperBoundObj is IAsyncNumber)
            {
                upperBound = (int)await ((IAsyncNumber)upperBoundObj).Value;
            }

            int i = 0;

            /*
                -2

                1,2,3

                negI = 1
             */
            foreach (var item in list)
            {
                var currI = i++;
                if (lowerBound.HasValue)
                {
                    if (lowerBound > 0 && lowerBound > currI) continue;
                    else if (lowerBound < 0 && (list.Count + lowerBound) > currI) continue;
                }
                if (upperBound.HasValue)
                {
                    if (upperBound > 0 && upperBound <= currI) break;
                    if (upperBound < 0 && (list.Count + upperBound) <= currI) break;
                }

                result.Add(item);
            }

            return new AsyncArray(result);
       }

       public override string ToString()
       {
           var lower = _lowerBound?.ToString() ?? "";
           var upper = _upperBound?.ToString() ?? "";

           return $"[{lower}..{upper}]";
       }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            // This can't be simplified
            return this;
        }

       public override bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables) => true;
    }
}