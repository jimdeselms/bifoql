using System.Collections.Generic;
using Bifoql.Expressions;

namespace Bifoql.Visitors
{
    internal class VariableReferenceFinder : ExprVisitor
    {
        private HashSet<string> _referencedVariables = new HashSet<string>();

        public override void Visit(VariableExpr expr)
        {
            _referencedVariables.Add(expr.Name);
        }

        public static IEnumerable<string> GetReferencedVariables(Expr e)
        {
            var finder = new VariableReferenceFinder();
            e.Accept(finder);
            return finder._referencedVariables;
        }        
    }
}