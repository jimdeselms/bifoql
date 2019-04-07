namespace Bifoql.Expressions
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Extensions;
    using Bifoql.Types;
    using Bifoql.Visitors;

    internal class ArrayExpr : Expr
    {
        private readonly IReadOnlyList<Expr> _exprs;

        public ArrayExpr(Location location, IReadOnlyList<Expr> exprs) : base(location)
        {
            _exprs = exprs;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new ArrayExpr(Location, _exprs.Select(e => e.Simplify(variables)).ToList());
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var tasks = _exprs.Select(e => new { spread= e is SpreadExpr, task = e.Apply(context)}).ToList();

            var resolvedExprs = await Task.WhenAll(tasks.Select(t => t.task));

            var result = new List<Func<Task<IBifoqlObject>>>();

            var types = new List<BifoqlType>();

            for (int currIdx = 0; currIdx < tasks.Count; currIdx++)
            {
                // Gotta copy the thing so that it can be captured in an async context.
                var i = currIdx;

                if (tasks[i].spread)
                {
                    var spreadList = resolvedExprs[i] as IBifoqlArrayInternal;
                    if (spreadList == null) return new AsyncError(this.Location, "In an array, spread expression must resolve to an array");
                    foreach (var item in spreadList)
                    {
                        result.Add(item);
                    }
                }
                else
                {
                    result.Add(() => Task.FromResult(resolvedExprs[i]));
                }
            }

            return new AsyncArray(result);
        }

        public override string ToString()
        {
            return $"[{string.Join(",", _exprs.Select(e => e.ToString()))}]";
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var expr in _exprs)
            {
                expr.Accept(visitor);
            }
        }
    }
}