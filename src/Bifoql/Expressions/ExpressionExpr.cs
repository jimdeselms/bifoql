namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class ExpressionExpr : Expr
    {
        public  Expr InnerExpression { get; }

        // Should the location technically be the parenthesis?
        public ExpressionExpr(Expr innerExpression) : base(innerExpression.Location)
        {
            InnerExpression = innerExpression;
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
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

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => InnerExpression.NeedsAsync(variables);
    }
}