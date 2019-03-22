using System.Collections.Generic;
using System.Threading.Tasks;
using Bifoql.Expressions;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncExpression : AsyncObjectBase, IBifoqlExpression
    {
        private readonly Expr _expr;

        public AsyncExpression(Expr expr)
        {
            _expr = expr;
        }

        public Task<IBifoqlObject> Evaluate(QueryContext context)
        {
            return _expr.Apply(context);
        }

        public Task<bool> IsEqualTo(IBifoqlObject o)
        {
            var other = o as AsyncExpression;
            return Task.FromResult(other?._expr == this._expr);
        }
    }
}