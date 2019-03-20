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

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            return new ExpressionExpr(InnerExpression.Simplify(variables));
        }

        protected override Task<IAsyncObject> DoApply(QueryContext context)
        {
            return Task.FromResult<IAsyncObject>(new AsyncExpression(InnerExpression));
        }

        public override string ToString()
        {
            return $"({InnerExpression.ToString()})";
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables) => InnerExpression.NeedsAsync(variables);
    }
}