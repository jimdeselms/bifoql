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

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
           return new TypeExpr(Location, new[] { _obj.Simplify(variables) });
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var val = await _obj.Apply(context);
            string type = "undefined";
            if (val == null || val is IBifoqlNull)
            {
                type = "null";
            }
            else if (val is IBifoqlBoolean)
            {
                type = "boolean";
            }
            else if (val is IBifoqlError)
            {
                type = "error";
            }
            else if (val is IBifoqlArray)
            {
                type = "array";
            }
            else if (val is IBifoqlMap)
            {
                type = "object";
            }
            else if (val is IBifoqlString)
            {
                type = "string";
            }
            else if (val is IBifoqlNumber)
            {
                type = "number";
            }

            return new AsyncString(type);
        }
    }
}