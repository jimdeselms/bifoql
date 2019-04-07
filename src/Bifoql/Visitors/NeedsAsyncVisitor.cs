using System.Collections.Generic;
using System.Linq;
using Bifoql.Expressions;

namespace Bifoql.Visitors
{
    internal class NeedsAsyncVisitor : ExprVisitor
    {
        private bool _needsAsync = false;

        public static bool NeedsAsync(Expr expr)
        {
            var visitor = new NeedsAsyncVisitor();
            expr.Accept(visitor);
            return visitor._needsAsync;
        }

        public override void Visit(AssignmentExpr expr)
        {
            _needsAsync = true;
        }
    }
}