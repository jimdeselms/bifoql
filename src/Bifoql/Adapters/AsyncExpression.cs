using System.Collections.Generic;
using System.Threading.Tasks;
using Bifoql.Expressions;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncExpression : AsyncObjectBase, IAsyncExpression
    {
        private readonly Expr _expr;

        public AsyncExpression(Expr expr)
        {
            _expr = expr;
        }

        public Task<IAsyncObject> Evaluate(QueryContext context)
        {
            return _expr.Apply(context);
        }

        public Task<BifoqlType> GetSchema()
        {
            return Task.FromResult<BifoqlType>(BifoqlType.Unknown);
        }

        public Task<bool> IsEqualTo(IAsyncObject o)
        {
            var other = o as AsyncExpression;
            return Task.FromResult(other?._expr == this._expr);
        }
    }
}