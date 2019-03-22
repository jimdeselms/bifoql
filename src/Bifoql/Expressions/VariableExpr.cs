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
            if (context.Variables.TryGetValue(Name, out value))
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

        public override Expr Simplify(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            IBifoqlObject result;
            if (variables.TryGetValue(Name, out result))
            {
                return new LiteralExpr(Location, result);
            }
            else
            {
                return this;
            }
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            // this can't be simplified.
            return this;
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            // References to the root variable can't be simplified.
            return Name != "" && !variables.ContainsKey(Name);
        }

        public override bool ReferencesRootVariable => Name == "";
    }
}