using Bifoql;
using System.Linq;
using System.Collections.Generic;

namespace Bifoql.Expressions.Builtins
{
    using System;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;

    internal class TypeExpr : FunctionExpr
    {
        private Expr _obj;

        public TypeExpr(Location location, IReadOnlyList<Expr> arguments) : base(location, arguments, "type")
        {
            _obj = arguments[0];
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
           return new TypeExpr(Location, new[] { _obj.Simplify(variables) });
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            var val = await _obj.Apply(context);
            string type = "undefined";
            if (val == null || val is IAsyncNull)
            {
                type = "null";
            }
            else if (val is IAsyncBoolean)
            {
                type = "boolean";
            }
            else if (val is IAsyncError)
            {
                type = "error";
            }
            else if (val is IAsyncArray)
            {
                type = "array";
            }
            else if (val is IAsyncMap)
            {
                type = "object";
            }
            else if (val is IAsyncString)
            {
                type = "string";
            }
            else if (val is IAsyncNumber)
            {
                type = "number";
            }

            return new AsyncString(type);
        }
    }
}