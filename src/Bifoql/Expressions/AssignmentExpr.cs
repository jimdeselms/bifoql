namespace Bifoql.Expressions
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Visitors;

    internal class AssignmentExpr : Expr
    {
        public string Name;
        public readonly Expr Value;
        private readonly Expr _pipedInto;

        public AssignmentExpr(Location location, string name, Expr value, Expr pipedInto) : base(location)
        {
            Name = name;
            Value = value;
            _pipedInto = pipedInto;
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            IBifoqlObject ignored;
            if (context.Variables.TryGetValue(Name, out ignored))
            {
                return new AsyncError(this.Location, $"Can't change value of variable '${Name}'");
            }
            else
            {
                // Get the new value and add to the context as a variable, and replace the current object with that new value as well.
                var variableValue = await Value.Apply(context);
                var newContext = context.AddVariable(Name, variableValue);

                var simplified = _pipedInto.Simplify(newContext.Variables);

                return await simplified.Apply(newContext);
            }
        }

        public override string ToString()
        {
            return $"${Name} = {Value.ToString()}; {_pipedInto.ToString()}";
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            var simplifiedValue = Value.Simplify(variables);
            if (simplifiedValue is LiteralExpr)
            {
                var newScope = variables.AddVariable(Name, ((LiteralExpr)simplifiedValue).Literal);
                return new AssignmentExpr(Location, Name, simplifiedValue, _pipedInto.Simplify(newScope));
            }
            else if (simplifiedValue is ExpressionExpr)
            {
                var newScope = variables.AddVariable(Name, new AsyncExpression(((ExpressionExpr)simplifiedValue).InnerExpression));
                return new AssignmentExpr(Location, Name, simplifiedValue, _pipedInto.Simplify(newScope));
            }
            else
            {   
                return new AssignmentExpr(Location, Name, simplifiedValue, _pipedInto.Simplify(variables));
            }
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            Value.Accept(visitor);
            _pipedInto.Accept(visitor);
        }
    }
}