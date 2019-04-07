namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

    internal class IndexedLookupExpr : Expr
    {
        private readonly Expr _leftHandSide;
        private readonly IReadOnlyDictionary<string, Expr> _arguments;

        public IndexedLookupExpr(Expr leftHandSide, IReadOnlyDictionary<string, Expr> arguments) : base(leftHandSide.Location)
        {
            _leftHandSide = leftHandSide;
            _arguments = arguments;
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            _leftHandSide.Accept(visitor);
            foreach (var arg in _arguments.Values)
            {
                arg.Accept(visitor);
            }
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var lhs = await _leftHandSide.Apply(context);

            // Propagate error
            if (lhs is IBifoqlError) return lhs;

            var leftHandSide = lhs as IBifoqlIndexInternal;

            if (leftHandSide == null) return new AsyncError(this.Location, "Can't do index lookup on something that isn't an index");

            var args = await IndexArgumentList.Create(_arguments, context);

            // Propagate error if there were any errors in evaluating the argument list.
            if (args.ErrorResult != null)
            {
                return args.ErrorResult;
            }

            var resultObj = await leftHandSide.Lookup(args);
            return resultObj.ToBifoqlObject();
        }

        public override string ToString()
        {
            var entries = string.Join(",", _arguments.Select(a => $"{a.Key}: {a.Value.ToString()}"));
            return $"{_leftHandSide.ToString()}({string.Join(", ", entries)})";
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            var newArgs = _arguments.ToDictionary(p => p.Key, p => p.Value.Simplify(variables));
            return new IndexedLookupExpr(
                _leftHandSide.Simplify(variables),
                newArgs);
        }

        public override bool NeedsAsync(VariableScope variables) => NeedsAsyncVisitor.NeedsAsync(this, variables);
    }
}