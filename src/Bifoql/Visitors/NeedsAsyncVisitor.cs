using System.Collections.Generic;
using System.Linq;
using Bifoql.Expressions;

namespace Bifoql.Visitors
{
    internal class NeedsAsyncVisitor : ExprVisitor
    {
        private bool _needsAsync = false;
        private readonly VariableScope _variables;

        public static bool NeedsAsync(Expr expr, VariableScope variables)
        {
            var visitor = new NeedsAsyncVisitor(variables);
            expr.Accept(visitor);
            return visitor._needsAsync;
        }

        private NeedsAsyncVisitor(VariableScope variables)
        {
            _variables = variables;
        }

        public override void Visit(VariableExpr expr)
        {
            if (expr.Name == "" || !_variables.ContainsKey(expr.Name))
            {
                _needsAsync = true;
            }
        }

        public override void Visit(TypedFunctionCallExpr expr)
        {
            if (expr.Name == "eval")
            {
                _needsAsync = true;
            }
        }

        public override void Visit(SliceExpr expr)
        {
            if (expr.Target == null)
            {
                _needsAsync = true;
            }
        }

        public override void Visit(KeyExpr expr)
        {
            if (expr.Target == null)
            {
                _needsAsync = true;
            }
        }

        public override void Visit(IndexExpr expr)
        {
            if (expr.Target == null)
            {
                _needsAsync = true;
            }
        }

        public override void Visit(ChainExpr expr)
        {
            if (ReferencesRootVariableVisitor.ReferencesRootVariable(expr))
            {
                _needsAsync = true;
                return;
            }

            // Some things can't be simplified by themselves, but they can be in the
            // context of a chain. If this chain doesn't need to be asynchronous, then
            // the next key or index won't need it either.
            if (NeedsAsyncByItselfVisitor.NeedsAsyncByItself(expr.Next))
            {
                _needsAsync = true;
                return;
            }
        }

        public override void Visit(AssignmentExpr expr) => _needsAsync = true;
        public override void Visit(IdentityExpr expr) => _needsAsync = true;
        public override void Visit(FilterExpr expr) => _needsAsync = true;
    }
}