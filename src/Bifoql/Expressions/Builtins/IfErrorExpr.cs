using Bifoql;
using System.Linq;
using System.Collections.Generic;

namespace Bifoql.Expressions.Builtins
{
    using System;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;

    internal class IfErrorExpr : FunctionExpr
    {
        private Expr _obj;
        private Expr _ifIsError;

        public IfErrorExpr(Location location, IReadOnlyList<Expr> arguments) : base(location, arguments, "if_error")
        {
            _obj = arguments[0];
            _ifIsError = arguments[1];
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
           return new IfErrorExpr(Location, new[] 
            { 
               _obj.Simplify(variables),
               _ifIsError.Simplify(variables)
            });
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            // Since this function essentialy works as a "catch", then we want to handle any unexpected errors as well.
            try
            {
                var obj = await _obj.Apply(context);
                if (!(obj is IAsyncError)) return obj;
            }
            catch
            {
                // Nothing to do here.
            }

            return await _ifIsError.Apply(context);
        }
    }
}