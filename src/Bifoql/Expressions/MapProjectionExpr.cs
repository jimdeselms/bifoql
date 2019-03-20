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
        private readonly IReadOnlyList<Expr> _projections;

        public MapProjectionExpr(Location location, IEnumerable<Expr> projections) : base(location)
        {
            _projections = projections.ToList();
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            return new MapProjectionExpr(Location, _projections.Select(p => p.Simplify(variables)));
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
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
                    var lookup = ((LiteralExpr)projection).Literal as IBifoqlMap;
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
            var thingToSpread = await spread.SpreadObject.Apply(context) as IBifoqlMap;
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

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => _projections.Any(a => a.NeedsAsync(variables));
        public override bool NeedsAsyncByItself => true;    
    }
}