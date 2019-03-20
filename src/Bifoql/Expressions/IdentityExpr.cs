namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class IdentityExpr : Expr
    {
        public IdentityExpr(Location location) : base(location)
        {
        }

        protected override Task<IBifoqlObject> DoApply(QueryContext context)
        {
            return Task.FromResult(context.QueryTarget);
        }

        public override string ToString()
        {
            return "@";
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            // This can't be simplified.
            return this;
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => true;
    }
}