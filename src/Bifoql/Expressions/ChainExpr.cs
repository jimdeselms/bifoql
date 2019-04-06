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
        private readonly Expr _first;
        private readonly Expr _next;
        private readonly ChainBehavior _chainBehavior;

        public ChainExpr(Expr first, Expr next, ChainBehavior chainBehavior) : base(first.Location)
        {
            _first = first;
            _next = next;
            _chainBehavior = chainBehavior;
        }

        protected override Expr SimplifyChildren(VariableScope variables)
        {
            return new ChainExpr(_first?.Simplify(variables), _next?.Simplify(variables), _chainBehavior);
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var result = await _first.Apply(context, resolveDeferred: false);

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
                    resultList.Add(() => _next.Apply(newContext));
                }

                return new AsyncArray(resultList);
            }
            else
            {
                return _next == null ? result : await _next.Apply(context.ReplaceTarget(result));
            }
        }

        public override string ToString()
        {
            var result = _first.ToString();

            return $"{result}{RightHandSideString()}";
        }

        private string RightHandSideString()
        {
            var result = "";

            if (_next != null)
            {
                if (_next is KeyExpr)
                {
                    var key = _next.ToString();
                    result += key.StartsWith("[") ? key : "." + key;
                }
                else if (_next is FilterExpr || _next is IndexedLookupExpr)
                {
                    result += _next.ToString();
                }
                else
                {
                    var pipe = _chainBehavior == ChainBehavior.ToMultiple ? " |< "
                        : _chainBehavior == ChainBehavior.OneToOne ? " | "
                        : " ";

                    result += pipe + _next.ToString();
                }
            }

            return result;
        }

        public override bool NeedsAsync(VariableScope variables) 
        {
            if (_first.NeedsAsync(variables)) return true;

            if (_first.ReferencesRootVariable || _next.ReferencesRootVariable) return true;

            // Some things can't be simplified by themselves, but they can be in the
            // context of a chain. If this chain doesn't need to be asynchronous, then
            // the next key or index won't need it either.
            if (_next.NeedsAsyncByItself) return false;

            return !_next.NeedsAsyncByItself && !_next.NeedsAsync(variables);
        }

        internal override void Accept(ExprVisitor visitor)
        {
            visitor.Visit(this);
            _first.Accept(visitor);
            _next.Accept(visitor);
        }

        public override bool ReferencesRootVariable => _first.ReferencesRootVariable || _next.ReferencesRootVariable;
    }
}