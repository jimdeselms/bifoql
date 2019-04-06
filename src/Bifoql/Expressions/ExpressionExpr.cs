namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Visitors;

    internal class ExpressionExpr : Expr
    {
        public  Expr InnerExpression { get; }

        // Should the location technically be the parenthesis?
        public ExpressionExpr(Expr innerExpression) : base(innerExpression.Location)
        {
            InnerExpression = innerExpression;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new ExpressionExpr(InnerExpression.Simplify(variables));
        }

        protected override Task<IBifoqlObject> DoApply(QueryContext context)
        {
            return Task.FromResult<IBifoqlObject>(new AsyncExpression(InnerExpression));
        }

        public override string ToString()
        {
            return $"({InnerExpression.ToString()})";
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            InnerExpression.Accept(visitor);
        }

        public override bool NeedsAsync(VariableScope variables) => InnerExpression.NeedsAsync(variables);

        public override bool ReferencesRootVariable => InnerExpression.ReferencesRootVariable;
    }
}