namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Visitors;

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

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            // This can't be simplified.
            return this;
        }

        public override bool NeedsAsync(VariableScope variables) => NeedsAsyncVisitor.NeedsAsync(this, variables);
    }
}