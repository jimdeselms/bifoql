
namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

    internal abstract class Expr
    {
        internal Location Location { get; }

        protected Expr(Location? location)
        {
            Location = location ?? new Location(0, 0);
        }

        public virtual async Task<IBifoqlObject> Apply(QueryContext context, bool resolveDeferred=true)
        {
            if (context.QueryTarget is IBifoqlError)
            {
                return context.QueryTarget;
            }

            if (context.QueryTarget is IBifoqlDeferredQueryInternal)
            {
                // A deferred query is a query that won't actually be evaluated here, but by some other
                // service. For example, let's say that I have another REST service that provides a BifoQL
                // endpoint. I can take this query and pass it along to that endpoint and get the result back.
                var deferred = ((IBifoqlDeferredQueryInternal)context.QueryTarget);
                var query = "@ | " + this.ToString();

                var bifoqlObject = await deferred.EvaluateQuery(query);
                return bifoqlObject.ToBifoqlObject();
            }

            var result = await DoApply(context);

            if (resolveDeferred && result is IBifoqlDeferredQueryInternal)
            {
                result = await DeferredQueryWrapper.EvaluateDeferredQuery(result);
            }

            return result;
        }

        protected abstract Task<IBifoqlObject> DoApply(QueryContext context);

        public virtual bool IsConstant => false;
        public virtual Expr Simplify(VariableScope scope)
        {
            if (IsConstant) return this;
            
            if (NeedsAsyncVisitor.NeedsAsync(this, scope))
            {
                return SimplifyChildren(scope);
            }
            else
            {
                // Since this thing can be simplified in real time, lk
                return new LiteralExpr(Location, this.Apply(new QueryContext(AsyncNull.Instance, scope)).Result);
            }
        }

        // Derived classes that need a context to be evaluated
        // must override this method to return a new expression that has the simplified versions of their children.
        protected virtual Expr SimplifyChildren(VariableScope scope)
        {
            if (NeedsAsyncVisitor.NeedsAsync(this, scope))
            {
                throw new NotImplementedException("Must implement SimplifyChildren");
            }
            else
            {
                return this;
            }
        }

        internal abstract void Accept(ExprVisitor visitor);
    }
}