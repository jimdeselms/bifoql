namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    internal class LiteralExpr : Expr
    {
        public IBifoqlObject Literal { get; }

        public LiteralExpr(Location location, IBifoqlObject literal) : base(location)
        {
            Literal = literal;
        }

        protected override Task<IBifoqlObject> DoApply(QueryContext context)
        {
            return Task.FromResult(Literal);
        }

        public override bool IsConstant => true;

        public override string ToString()
        {
            if (Literal is IBifoqlBoolean)
            {
                var val = ((IBifoqlBoolean)Literal).Value.Result;
                return val.ToString();
            }
            else if (Literal is IBifoqlNumber)
            {
                var val = ((IBifoqlNumber)Literal).Value.Result;
                return val.ToString();
            }
            else if (Literal is IBifoqlString)
            {
                var val = ((IBifoqlString)Literal).Value.Result;
                return "\"" + val + "\"";
            }
            else if (Literal is IBifoqlError)
            {
                return $"<error: {((IBifoqlError)Literal).Message}>";
            }
            else if (Literal is IBifoqlNull)
            {
                return "null";
            }
            else
            {
                return "{{{some other kind of literal}}}";
            }
        }

        public override bool NeedsAsync(VariableScope variables) => false;
        public override bool ReferencesRootVariable => false;
    }
}