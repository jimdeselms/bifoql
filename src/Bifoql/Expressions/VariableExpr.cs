namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class VariableExpr : Expr
    {
        public string Name;

        public VariableExpr(Location location, string name) : base(location)
        {
            Name = name;
        }

        protected override Task<IBifoqlObject> DoApply(QueryContext context)
        {
            IBifoqlObject value;
            if (context.Variables.TryGetValue(Name.ToLower(), out value))
            {
                return Task.FromResult(value);
            }
            else
            {
                return Task.FromResult<IBifoqlObject>(new AsyncError(this.Location, $"Variable '${Name}' not found"));
            }
        }

        public override string ToString()
        {
            return "$" + Name;
        }

        public override Expr Simplify(VariableScope scope)
        {
            IBifoqlObject result;
            if (scope.TryGetValue(Name, out result))
            {
                return new LiteralExpr(Location, result);
            }
            else
            {
                return this;
            }
        }

        protected override Expr SimplifyChildren(VariableScope scope)
        {
            // this can't be simplified.
            return this;
        }

        public override bool NeedsAsync(VariableScope scope)
        {
            // References to the root variable can't be simplified.
            return Name == "" || !scope.ContainsKey(Name);
        }

        public override bool ReferencesRootVariable => Name == "";
    }
}