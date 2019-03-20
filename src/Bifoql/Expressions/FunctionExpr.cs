namespace Bifoql.Expressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    internal abstract class FunctionExpr : Expr
    {
        protected string Name { get; }
        protected IReadOnlyList<Expr> Arguments { get; private set; }

        public FunctionExpr(Location location, IEnumerable<Expr> arguments, string name) : base(location)
        {
            Arguments = arguments.ToList();
            Name = name;
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"{Name}({args})";
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => Arguments.Any(a => a.NeedsAsync(variables));
    }
}