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

        public override void Visit(AssignmentExpr expr)
        {
            _needsAsync = true;
        }
    }
}