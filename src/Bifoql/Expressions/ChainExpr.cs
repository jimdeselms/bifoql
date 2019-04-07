namespace Bifoql.Expressions
{
    using System.Threading.Tasks;
    using System.Linq;
    using System.Collections.Generic;
    using Bifoql.Adapters;
    using System;
    using Bifoql.Extensions;
    using Bifoql.Visitors;

    internal enum ChainBehavior
    {
        OneToOne,
        ToMultiple,
        ToMultipleIfArray
    }

    internal class ChainExpr : Expr
    {
        internal readonly Expr First;
        internal readonly Expr Next;
        private readonly ChainBehavior _chainBehavior;

        public ChainExpr(Expr first, Expr next, ChainBehavior chainBehavior) : base(first.Location)
        {
            First = first;
            Next = next;
            _chainBehavior = chainBehavior;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new ChainExpr(First?.Simplify(variables), Next?.Simplify(variables), _chainBehavior);
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var result = await First.Apply(context, resolveDeferred: false);

            result = await result.GetDefaultValue();

            // Propagate errors.
            if (result is IBifoqlError) return result;

            var deferred = result as IBifoqlDeferredQueryInternal;
            if (deferred != null)
            {
                return DeferredQueryWrapper.AddToQuery(deferred, RightHandSideString());
            }

            var array = result as IBifoqlArrayInternal;
            if (_chainBehavior == ChainBehavior.ToMultiple || (array != null && _chainBehavior == ChainBehavior.ToMultipleIfArray))
            {
                if (array == null) return new AsyncError(this.Location, "pipe to multiple only works on an array");

                var resultList = new List<Func<Task<IBifoqlObject>>>();

                foreach (var entry in array)
                {
                    var entryValue = await entry();
                    var newContext = context.ReplaceTarget(entryValue);
                    resultList.Add(() => Next.Apply(newContext));
                }

                return new AsyncArray(resultList);
            }
            else
            {
                return Next == null ? result : await Next.Apply(context.ReplaceTarget(result));
            }
        }

        public override string ToString()
        {
            var result = First.ToString();

            return $"{result}{RightHandSideString()}";
        }

        private string RightHandSideString()
        {
            var result = "";

            if (Next != null)
            {
                if (Next is KeyExpr)
                {
                    var key = Next.ToString();
                    result += key.StartsWith("[") ? key : "." + key;
                }
                else if (Next is FilterExpr || Next is IndexedLookupExpr)
                {
                    result += Next.ToString();
                }
                else
                {
                    var pipe = _chainBehavior == ChainBehavior.ToMultiple ? " |< "
                        : _chainBehavior == ChainBehavior.OneToOne ? " | "
                        : " ";

                    result += pipe + Next.ToString();
                }
            }

            return result;
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            First.Accept(visitor);
            Next.Accept(visitor);
        }
    }
}