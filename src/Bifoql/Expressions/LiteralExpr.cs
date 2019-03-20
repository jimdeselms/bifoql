namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    internal class LiteralExpr : Expr
    {
        public IAsyncObject Literal { get; }

        public LiteralExpr(Location location, IAsyncObject literal) : base(location)
        {
            Literal = literal;
        }

        protected override Task<IAsyncObject> DoApply(QueryContext context)
        {
            return Task.FromResult(Literal);
        }

        public override bool IsConstant => true;

        public override string ToString()
        {
            if (Literal is IAsyncBoolean)
            {
                var val = ((IAsyncBoolean)Literal).Value.Result;
                return val.ToString();
            }
            else if (Literal is IAsyncNumber)
            {
                var val = ((IAsyncNumber)Literal).Value.Result;
                return val.ToString();
            }
            else if (Literal is IAsyncString)
            {
                var val = ((IAsyncString)Literal).Value.Result;
                return "\"" + val + "\"";
            }
            else if (Literal is IAsyncError)
            {
                return $"<error: {((IAsyncError)Literal).Message}>";
            }
            else if (Literal is IAsyncNull)
            {
                return "null";
            }
            else
            {
                return "{{{some other kind of literal}}}";
            }
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables) => false;
    }
}