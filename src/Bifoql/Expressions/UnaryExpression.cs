namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

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
            value = await value.GetDefaultValue();

            // Propagate error.
            if (value is IBifoqlError) return value;
            
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
            else if (_operator == "!")
            {
                var b = value as IBifoqlBoolean;
                if (b != null)
                {
                    var val = await b.Value;
                    return new AsyncBoolean(!val);
                }
                else if (value is IBifoqlNull || value is IBifoqlUndefined || value is IBifoqlError)
                {
                    return new AsyncBoolean(true);
                }
                else if (value is IBifoqlString)
                {
                    var val = await ((IBifoqlString)value).Value;
                    return new AsyncBoolean(string.IsNullOrEmpty(val));
                }
                else if (value is IBifoqlArrayInternal)
                {
                    var count = ((IBifoqlArrayInternal)value).Count;
                    return new AsyncBoolean(count == 0);
                }
                else
                {
                    return new AsyncBoolean(false);
                }
            }

            return new AsyncError(this.Location, $"Unknown operator {_operator}");
        }

        public override string ToString()
        {
            return $"{_operator}{_innerExpression.ToString()}";
        }

        public override bool NeedsAsync(VariableScope scope) => _innerExpression.NeedsAsync(scope);

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            _innerExpression.Accept(visitor);
        }

        public override bool ReferencesRootVariable => _innerExpression.ReferencesRootVariable;
    }
}