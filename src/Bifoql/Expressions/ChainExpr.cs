namespace Bifoql.Expressions
{
    using System.Threading.Tasks;
    using System.Linq;
    using System.Collections.Generic;
    using Bifoql.Adapters;
    using System;

    internal class ChainExpr : Expr
    {
        private readonly Expr _first;
        private readonly Expr _next;
        private readonly bool _toMultiple;

        public ChainExpr(Expr first, Expr next, bool toMultiple) : base(first.Location)
        {
            _first = first;
            _next = next;
            _toMultiple = toMultiple;
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IBifoqlObject> variables)
        {
            return new ChainExpr(_first?.Simplify(variables), _next?.Simplify(variables), _toMultiple);
        }

        protected override async Task<IBifoqlObject> DoApply(QueryContext context)
        {
            var result = await _first.Apply(context);

            if (_toMultiple)
            {
                var array = result as IBifoqlArrayInternal;
                if (array == null) return new AsyncError(this.Location, "pipe to multiple only works on an array");

                var resultList = new List<Func<Task<IBifoqlObject>>>();

                foreach (var entry in array)
                {
                    var newContext = context.ReplaceTarget(await entry());
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
                    result += " | " + _next.ToString();
                }
            }

            return result;
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IBifoqlObject> variables) 
        {
            if (_first.NeedsAsync(variables)) return true;

            if (_next.ReferencesRootVariable) return true;

            // Some things can't be simplified by themselves, but they can be in the
            // context of a chain. If this chain doesn't need to be asynchronous, then
            // the next key or index won't need it either.
            if (_next.NeedsAsyncByItself) return false;

            return !_next.NeedsAsyncByItself && !_next.NeedsAsync(variables);
        }
        public override bool ReferencesRootVariable => _first.ReferencesRootVariable || _next.ReferencesRootVariable;
    }
}