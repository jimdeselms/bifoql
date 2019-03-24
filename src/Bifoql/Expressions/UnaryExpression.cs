namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class UnaryExpr : Expr
    {
        private readonly string _operator;
        private readonly Expr _innerExpression;

        public UnaryExpr(Location location, string @operator, Expr innerExpression) : base(location)
        {
            _operator = @operator;
            _innerExpression = innerExpression;
        }

        protected override Expr SimplifyChildren(VariableScope scope)
        {
           return new UnaryExpr(Location, _operator, _innerExpression.Simplify(scope));
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var value = await _innerExpression.Apply(context);
            if (_operator == "-")
            {
                var num = value as IBifoqlNumber;
                if (num != null)
                {
                    var val = await num.Value;
                    return new AsyncNumber(-val);
                }
                else
                {
                    return new AsyncError(this.Location, "Can't take negative of non-number");
                }
            }

            return new AsyncError(this.Location, $"Unknown operator {_operator}");
        }

        public override string ToString()
        {
            return $"{_operator}{_innerExpression.ToString()}";
        }

        public override bool NeedsAsync(VariableScope scope) => _innerExpression.NeedsAsync(scope);
        public override bool ReferencesRootVariable => _innerExpression.ReferencesRootVariable;
    }
}