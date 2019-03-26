namespace Bifoql.Expressions
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Extensions;
    using Bifoql.Adapters;

    internal class MapProjectionExpr : Expr
    {
        private readonly Expr _target;
        private readonly IReadOnlyList<Expr> _projections;

        public MapProjectionExpr(Location location, Expr target, IEnumerable<Expr> projections) : base(location)
        {
            _target = target;
            _projections = projections.ToList();
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new MapProjectionExpr(
                Location,
                _target?.Simplify(variables),
                _projections.Select(p => p.Simplify(variables)));
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var target = _target == null
                ? context
                : context.ReplaceTarget(await _target.Apply(context));

            // If this is the first expression in a chain, then we will not
            // apply the map to every element of the array.
            // In other words:
            // [1, 2] | { x: @[0] + @[1] } => { x: 3 }
            //
            // But if we're applying the projection directly to a target,
            // then we do want to apply the map to every element:
            // [ { x:1, y: 2}, { x: 5, y: 10 }] { x } => [ {x:1}, {x:5}]
            if (_target != null && target.QueryTarget is IBifoqlArrayInternal)
            {
                var result = new List<Func<Task<IBifoqlObject>>>();
                foreach (var entry in ((IBifoqlArrayInternal)target.QueryTarget))
                {
                    var entryContext = target.ReplaceTarget(await entry());
                    result.Add(() => ApplyToDict(entryContext));
                }

                return new AsyncArray(result);
            }
            else
            {
                return await ApplyToDict(target);
            }
        }

        protected async Task<IBifoqlObject> ApplyToDict(QueryContext context)
        {
            try
            {
                var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>();
                await Apply(context, dict);

                // And now, just because, let's reverse these back to make the unit tests work.
                // We can make the unit tests smarter later.
                var newDict = new Dictionary<string, Func<Task<IBifoqlObject>>>();

                foreach (var pair in dict.Reverse())
                {
                    newDict[pair.Key] = pair.Value;
                }

                return new AsyncMap(newDict);
            }
            catch (Exception ex)
            {
                return new AsyncError(this.Location, ex.Message);
            }
        }

        private async Task Apply(QueryContext context, Dictionary<string, Func<Task<IBifoqlObject>>> dict)
        {
            foreach (var projection in _projections.Reverse())
            {
                var keyValuePair = projection as KeyValuePairExpr;
                if (keyValuePair != null)
                {
                    if (!dict.ContainsKey(keyValuePair.Key))
                    {
                        Func<Task<IBifoqlObject>> value = () => keyValuePair.Value.Apply(context);
                        dict[keyValuePair.Key] = value;
                    }
                    continue;
                }
                else if (projection is SpreadExpr)
                {
                    await ApplySpread((SpreadExpr)projection, context, dict);
                    continue;
                }
                else if (projection is LiteralExpr)
                {
                    // This seems a little hinky, but it gets the job done.
                    var lookup = ((LiteralExpr)projection).Literal as IBifoqlMapInternal;
                    if (lookup == null)
                    {
                        throw new Exception("Only key value pairs or spreads are allowed in a map projection");
                    }
                    else
                    {
                        foreach (var pair in lookup)
                        {
                            if (!dict.ContainsKey(pair.Key))
                            {
                                dict[pair.Key] = pair.Value;
                            }
                        }
                    }
                }
            }
        }

        private async Task ApplySpread(SpreadExpr spread, QueryContext context, Dictionary<string, Func<Task<IBifoqlObject>>> dictionary)
        {
            var thingToSpread = await spread.SpreadObject.Apply(context) as IBifoqlMapInternal;
            if (thingToSpread == null) throw new Exception("spread expression must evaluate to a map");

            foreach (var pair in thingToSpread)
            {
                if (!dictionary.ContainsKey(pair.Key))
                {
                    dictionary[pair.Key] = pair.Value;
                }
            }
        }

        public override string ToString()
        {
            var projections = string.Join(", ", _projections.Select(p => p.ToString()));
            return "{" + projections + "}";
        }

        public override bool NeedsAsync(VariableScope variables) => _projections.Any(a => a.NeedsAsync(variables));
        public override bool NeedsAsyncByItself => true;    
        public override bool ReferencesRootVariable => _projections.Any(p => p.ReferencesRootVariable);
    }
}