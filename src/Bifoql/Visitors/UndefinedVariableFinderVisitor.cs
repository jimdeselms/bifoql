using System.Collections.Generic;
using System.Linq;
using Bifoql.Expressions;

namespace Bifoql.Visitors
{
    internal class UndefinedVariableFinderVisitor : ExprVisitor
    {
        private HashSet<string> _definedVariables;
        private List<Expr> _undefinedVariableReferences = new List<Expr>();

        public override void Visit(VariableExpr expr)
        {
            if (!_definedVariables.Contains(expr.Name))
            {
                _undefinedVariableReferences.Add(expr);
            }
        }

        private UndefinedVariableFinderVisitor(string[] declaredVariables)
        {
            _definedVariables = new HashSet<string>(declaredVariables);
            _definedVariables.Add("");
        }

        public static IEnumerable<Expr> GetUndefinedVariableReferences(Expr expr, string[] declaredVariables)
        {
            var visitor = new UndefinedVariableFinderVisitor(declaredVariables);
            expr.Accept(visitor);
            return visitor._undefinedVariableReferences;
        }

        public override void Visit(AssignmentExpr expr)
        {
            // If the expression does not reference itself, then we're cool.
            var refs = VariableReferenceFinder.GetReferencedVariables(expr.Value);
            if (refs.All(r => r != expr.Name))
            {
                _definedVariables.Add(expr.Name);
            }
        }
    }
}