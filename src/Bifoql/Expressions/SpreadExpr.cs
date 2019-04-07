namespace Bifoql.Expressions
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Bifoql.Adapters;
    using Bifoql.Visitors;

    internal class SpreadExpr : Expr
    {
        public Expr SpreadObject { get; }

        public SpreadExpr(Location location, Expr spreadObject) : base(location)
        {
            SpreadObject = spreadObject;
        }
        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            // It's up to the caller to take this resolved object and apply it to the list or array.
            return await SpreadObject.Apply(context);
        }

       public override string ToString()
       {
           return $"...{SpreadObject.ToString()}";
       }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new SpreadExpr(Location, SpreadObject.Simplify(variables));
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            SpreadObject.Accept(visitor);
        }
    }
}