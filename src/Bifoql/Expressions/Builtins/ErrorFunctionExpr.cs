namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;

    internal class ErrorFunctionExpr : FunctionExpr
    {
        private readonly Expr _message;

        public ErrorFunctionExpr(Location location, IReadOnlyList<Expr> arguments) : base(location, arguments, "error")
        {
            _message = arguments[0];
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var message = await _message.Apply(context);
            var messageObj = await message.ToSimpleObject();

            return new AsyncError(this.Location, messageObj.ToString());
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            return new ErrorFunctionExpr(Location, new Expr[] { _message.Simplify(variables) } );
        }

        public override string ToString()
        {
            return $"<error: {_message}>";
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) => _message.NeedsAsync(variables);
    }
}