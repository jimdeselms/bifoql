
namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;

    internal abstract class Expr
    {
        internal Location Location { get; }

        protected Expr(Location? location)
        {
            Location = location ?? new Location(0, 0);
        }

        public virtual async Task<IAsyncObject> Apply(QueryContext context)
        {
            if (context.QueryTarget is ErrorExpr)
            {
                return context.QueryTarget;
            }

            if (context.QueryTarget is IAsyncDeferredQuery)
            {
                // A deferred query is a query that won't actually be evaluated here, but by some other
                // service. For example, let's say that I have another REST service that provides a BifoQL
                // endpoint. I can take this query and pass it along to that endpoint and get the result back.
                var deferred = ((IAsyncDeferredQuery)context.QueryTarget);
                var query = this.ToString();

                // Super cheesy.
                // TODO
                if (this is KeyExpr)
                {
                    query = "@." + query;
                }
                else if (this is FilterExpr || this is IndexedLookupExpr)
                {
                    query = "@" + query;
                }
                else
                {
                    query = "@ | " + query;
                }

                return await deferred.EvaluateQuery(query);
            }

            return await DoApply(context);
        }

        protected abstract Task<IAsyncObject> DoApply(QueryContext context);

        // True if it can be simplified synchronously without having a context applied to it.
        public abstract bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables);
        public virtual bool NeedsAsyncByItself => false;
        public virtual bool IsConstant => false;
        public virtual Expr Simplify(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            if (NeedsAsync(variables))
            {
                return SimplifyChildren(variables);
            }
            else
            {
                // Since this thing can be simplified in real time, lk
                return new LiteralExpr(Location, this.Apply(new QueryContext(AsyncNull.Instance, variables)).Result);
            }
        }

        // Derived classes that need a context to be evaluated
        // must override this method to return a new expression that has the simplified versions of their children.
        protected virtual Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            if (NeedsAsync(variables))
            {
                throw new NotImplementedException("Must implement SimplifyChildren");
            }
            else
            {
                return this;
            }
        }
    }
}