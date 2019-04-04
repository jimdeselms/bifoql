using Bifoql.Adapters;
using Bifoql.Expressions;
using Bifoql.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bifoql
{
    public interface IIndexArgumentList
    {
        double? TryGetNumberParameter(string key);
        string TryGetStringParameter(string key);
        bool? TryGetBooleanParameter(string key);
        double[] TryGetNumberArrayParameter(string key);
        string[] TryGetStringArrayParameter(string key);
    }

    public class IndexArgumentList : IIndexArgumentList
    {
        private readonly IReadOnlyDictionary<string, object> _entries;
        internal IBifoqlError ErrorResult { get; }

        internal static IndexArgumentList CreateEmpty()
        {
            return new IndexArgumentList(new Dictionary<string, object>(), null);
        }

        internal static async Task<IndexArgumentList> Create(IReadOnlyDictionary<string, Expr> entries, QueryContext context)
        {
            var values = new Dictionary<string, object>();

            foreach (var pair in entries)
            {
                var value = await pair.Value.Apply(context);
                if (value is IBifoqlError)
                {
                    // This mechanism is a little clunky; this list is kind of a union type where the value is either an error or
                    // a list. Not gonna overthink it for now.
                    return new IndexArgumentList(null, (IBifoqlError)value);
                }
                values[pair.Key] = await value.ToSimpleObject();
            }

            return new IndexArgumentList(values, null);
        }

        private IndexArgumentList(IReadOnlyDictionary<string, object> entries, IBifoqlError errorResult)
        {
            _entries = entries;
            ErrorResult = errorResult;
        }

        public double? TryGetNumberParameter(string key)
        {
            object entry;
            if (_entries.TryGetValue(key, out entry))
            {
                return Convert.ToDouble(entry);
            }
            else
            {
                return null;
            }
        }

        public string TryGetStringParameter(string key)
        {
            object entry;
            if (_entries.TryGetValue(key, out entry))
            {
                return (string)entry;
            }
            else
            {
                return null;
            }
        }

        public bool? TryGetBooleanParameter(string key)
        {
            object entry;
            if (_entries.TryGetValue(key, out entry))
            {
                return Convert.ToBoolean(entry);
            }
            else
            {
                return null;
            }
        }

        public double[] TryGetNumberArrayParameter(string key)
        {
            object entry;
            if (_entries.TryGetValue(key, out entry))
            {
                return (double[])entry;
            }
            else
            {
                return null;
            }
        }

        public string[] TryGetStringArrayParameter(string key)
        {
            object entry;
            if (_entries.TryGetValue(key, out entry))
            {
                return (string[])entry;
            }
            else
            {
                return null;
            }
        }
    } 
}