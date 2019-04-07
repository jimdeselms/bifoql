using Bifoql;
using System.Linq;
using System.Collections.Generic;

namespace Bifoql.Expressions.Builtins
{
    using System;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

    internal class IfErrorExpr : FunctionExpr
    {
        private Expr _obj;
        private Expr _ifIsError;

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            _obj.Accept(visitor);
            _ifIsError.Accept(visitor);
        }

        public IfErrorExpr(Location location, IReadOnlyList<Expr> arguments) : base(location, arguments, "if_error")
        {
            _obj = arguments[0];
            _ifIsError = arguments[1];
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
           return new IfErrorExpr(Location, new[] 
            { 
               _obj.Simplify(variables),
               _ifIsError.Simplify(variables)
            });
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            // Since this function essentialy works as a "catch", then we want to handle any unexpected errors as well.
            try
            {
                var obj = await _obj.Apply(context);
                if (!(obj is IBifoqlError)) return obj;
            }
            catch
            {
                // Nothing to do here.
            }

            return await _ifIsError.Apply(context);
        }
    }
}