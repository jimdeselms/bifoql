namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Visitors;

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

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool NeedsAsync(VariableScope variables) => false;
        public override bool ReferencesRootVariable => false;
    }
}