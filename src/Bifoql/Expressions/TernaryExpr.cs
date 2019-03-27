namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal class TernaryExpr : Expr
    {
        private readonly Expr _condition;
        private readonly Expr _ifTrue;
        private readonly Expr _ifFalse;

        public TernaryExpr(Expr condition, Expr ifTrue, Expr ifFalse) : base(condition.Location)
        {
            _condition = condition;
            _ifTrue = ifTrue;
            _ifFalse = ifFalse;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new TernaryExpr(_condition.Simplify(variables), _ifTrue.Simplify(variables), _ifFalse.Simplify(variables));
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var condition = await _condition.Apply(context);

            // Propagate error
            if (condition is IBifoqlError) return condition;

            var boolExpr = condition as IBifoqlBoolean;
            if (boolExpr == null) return new AsyncError(this.Location, "Ternary expression must have boolean condition");

            if (await boolExpr.Value)
            {
                return await _ifTrue.Apply(context);
            }
            else
            {
                return await _ifFalse.Apply(context);
            }
        }

        public override string ToString()
        {
            return $"{_condition.ToString()} ? {_ifTrue.ToString()} : {_ifFalse.ToString()}";
        }

        public override bool NeedsAsync(VariableScope variables)
        {
            return _condition.NeedsAsync(variables) || _ifFalse.NeedsAsync(variables) || _ifTrue.NeedsAsync(variables);
        }

        public override bool ReferencesRootVariable => _condition.ReferencesRootVariable || _ifTrue.ReferencesRootVariable || _ifFalse.ReferencesRootVariable;
    }
}