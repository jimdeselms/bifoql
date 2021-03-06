namespace Bifoql.Expressions
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Bifoql.Adapters;
    using Bifoql.Visitors;

    internal class KeyValuePairExpr : Expr
    {
        public string Key { get; }
        public Expr Value { get; }

        public KeyValuePairExpr(Location location, string key, Expr value) : base(location)
        {
            Key = key;
            Value = value;
        }
        protected override Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>()
            {
                [Key] = () => Value.Apply(context)
            };

            return Task.FromResult<IBifoqlObject>(new AsyncLookup(dict, null));
        }
        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            Value.Accept(visitor);
        }

       public override string ToString()
       {
           return $"{Key}: {Value.ToString()}";
       }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new KeyValuePairExpr(Location, Key, Value.Simplify(variables));
        }
    }
}