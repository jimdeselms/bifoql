namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Visitors;

    internal abstract class FunctionExpr : Expr
    {
        protected string Name { get; }
        protected IReadOnlyList<Expr> Arguments { get; private set; }

        public FunctionExpr(Location location, IEnumerable<Expr> arguments, string name) : base(location)
        {
            Arguments = arguments.ToList();
            Name = name;
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var arg in Arguments)
            {
                arg.Accept(visitor);
            }
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"{Name}({args})";
        }
    }
}