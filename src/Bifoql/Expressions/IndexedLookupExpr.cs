namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;

    internal class IndexedLookupExpr : Expr
    {
        private readonly Expr _leftHandSide;
        private readonly IReadOnlyDictionary<string, Expr> _arguments;

        public IndexedLookupExpr(Expr leftHandSide, IReadOnlyDictionary<string, Expr> arguments) : base(leftHandSide.Location)
        {
            _leftHandSide = leftHandSide;
            _arguments = arguments;
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var leftHandSide = await _leftHandSide.Apply(context) as IBifoqlIndex;
            if (leftHandSide == null) return new AsyncError(this.Location, "Can't do index lookup on something that isn't an index");

            var args = new IndexArgumentList(_arguments, context);
            var resultObj = await leftHandSide.Lookup(args);
            return resultObj.ToAsyncObject();
        }

        public override string ToString()
        {
            var entries = string.Join(",", _arguments.Select(a => $"{a.Key}: {a.Value.ToString()}"));
            return $"{_leftHandSide.ToString()}({string.Join(", ", entries)})";
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            var newArgs = _arguments.ToDictionary(p => p.Key, p => p.Value.Simplify(variables));
            return new IndexedLookupExpr(
                _leftHandSide.Simplify(variables),
                newArgs);
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) 
        {
            return _leftHandSide.NeedsAsync(variables) || _arguments.Any(p => p.Value.NeedsAsync(variables));
        }

        public override bool NeedsAsyncByItself => true;
    }
}