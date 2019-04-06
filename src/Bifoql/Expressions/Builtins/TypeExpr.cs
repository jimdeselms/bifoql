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

    internal class TypeExpr : FunctionExpr
    {
        private Expr _obj;

        public TypeExpr(Location location, IReadOnlyList<Expr> arguments) : base(location, arguments, "type")
        {
            _obj = arguments[0];
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            _obj.Accept(visitor);
        }

        protected override Expr SimplifyChildren(VariableScope variables)
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
            else if (val is IBifoqlArrayInternal)
            {
                type = "array";
            }
            else if (val is IBifoqlMapInternal)
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
        public override bool ReferencesRootVariable => _obj.ReferencesRootVariable;
    }
}