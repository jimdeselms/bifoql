namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class ErrorExpr : Expr
    {
        private readonly string _message;

        public ErrorExpr(Location location, string message) : base(location)
        {
            _message = message;
        }

        protected override Task<IBifoqlObject> DoApply(QueryContext context)
        {
            return Task.FromResult<IBifoqlObject>(new AsyncError(this.Location, _message));
        }


        public override string ToString()
        {
            if (Location.Line > 0)
            {
                return $"<error ({Location.Line}, {Location.Column}): {_message}>";
            }
            else
            {
                return $"<error: {_message}>";
            }
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => false;
        public override bool ReferencesRootVariable => false;
    }
}