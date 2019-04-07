using System.Collections.Generic;
using System.Linq;
using Bifoql.Expressions;

namespace Bifoql.Visitors
{
    internal class NeedsAsyncByItselfVisitor : ExprVisitor
    {
        private bool _needsAsyncByItself = false;

        public static bool NeedsAsyncByItself(Expr expr)
        {
            var visitor = new NeedsAsyncByItselfVisitor();
            expr.Accept(visitor);
            return visitor._needsAsyncByItself;
        }

        public override void Visit(IndexedLookupExpr expr) => _needsAsyncByItself = true;
        public override void Visit(IndexExpr expr) => _needsAsyncByItself = true;
        public override void Visit(MapProjectionExpr expr) => _needsAsyncByItself = true;
        public override void Visit(KeyExpr expr) => _needsAsyncByItself = expr.Target == null ? true : _needsAsyncByItself;
    }
}