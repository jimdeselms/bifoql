using Bifoql.Adapters;
using Bifoql.Expressions;
using Bifoql.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bifoql
{
    public class IndexArgumentList
    {
        private readonly IReadOnlyDictionary<string, Expr> _entries;
        private readonly QueryContext _context;

        internal IndexArgumentList(IReadOnlyDictionary<string, Expr> entries, QueryContext context)
        {
            _entries = entries;
            _context = context;
        }

        public async Task<double?> TryGetNumberParameter(string key)
        {
            Expr entry;
            if (_entries.TryGetValue(key, out entry))
            {
                var asyncObj = await entry.Apply(_context);
                var value = await asyncObj.ToSimpleObject();
                return Convert.ToDouble(value);
            }
            else
            {
                return null;
            }
        }

        public async Task<string> TryGetStringParameter(string key)
        {
            Expr entry;
            if (_entries.TryGetValue(key, out entry))
            {
                var asyncObj = await entry.Apply(_context);
                var value = await asyncObj.ToSimpleObject();
                return (string)value;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool?> TryGetBooleanParameter(string key)
        {
            Expr entry;
            if (_entries.TryGetValue(key, out entry))
            {
                var asyncObj = await entry.Apply(_context);
                var value = await asyncObj.ToSimpleObject();
                return (bool)value;
            }
            else
            {
                return null;
            }
        }

        public async Task<double[]> TryGetNumberArrayParameter(string key)
        {
            var param = await TryGetParameter(key) as IBifoqlArray;
            if (param == null)
            {
                return null;
            }

            var result = new List<double>();
            foreach (var obj in param)
            {
                var val = await (await obj()).ToSimpleObject();
                result.Add(Convert.ToDouble(val));
            }

            return result.ToArray();
        }

        public async Task<string[]> TryGetStringArrayParameter(string key)
        {
            var param = await TryGetParameter(key) as IBifoqlArray;
            if (param == null)
            {
                return null;
            }

            var result = new List<string>();
            foreach (var obj in param)
            {
                var val = await (await obj()).ToSimpleObject();
                if (!(val is string)) return null;
                result.Add((string)val);
            }

            return result.ToArray();
        }

        public async Task<IBifoqlObject> TryGetParameter(string key)
        {
            Expr entry;
            if (_entries.TryGetValue(key, out entry))
            {
                var asyncObj = await entry.Apply(_context);
                return asyncObj;
            }
            else
            {
                return null;
            }
        }
    } 
}