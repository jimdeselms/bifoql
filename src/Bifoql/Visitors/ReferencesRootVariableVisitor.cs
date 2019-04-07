using System.Collections.Generic;
using System.Linq;
using Bifoql.Expressions;

namespace Bifoql.Visitors
{
    internal class ReferencesRootVariableVisitor : ExprVisitor
    {
        private bool _referencesRootVariable = false;

        public override void Visit(VariableExpr expr)
        {
            if (expr.Name == "")
            {
                _referencesRootVariable = true;
            }
        }

        public static bool ReferencesRootVariable(Expr expr)
        {
            var visitor = new ReferencesRootVariableVisitor();
            expr.Accept(visitor);
            return visitor._referencesRootVariable;
        }
    }
}