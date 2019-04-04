using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bifoql.Adapters;
using Bifoql.Types;

namespace Bifoql.Extensions
{
    internal static class ObjectConverter
    {
        internal static Task<object> ToSimpleObject(this IBifoqlObject o)
        {
            var defaultVal = (o as IBifoqlHasDefaultValue)?.GetDefaultValue();
            if (defaultVal != null)
            {
                return ToSimpleObject(defaultVal);
            }

            var map = o as IBifoqlMapInternal;
            if (map != null) return ToSimpleObject(map);

            var arr = o as IBifoqlArrayInternal;
            if (arr != null) return ToSimpleObject(arr);

            var str = o as IBifoqlString;
            if (str != null) return ToSimpleObject(str);

            var num = o as IBifoqlNumber;
            if (num != null) return ToSimpleObject(num);

            var boolean = o as IBifoqlBoolean;
            if (boolean != null) return ToSimpleObject(boolean);

            var deferred = o as IBifoqlDeferredQueryInternal;
            if (deferred != null) return ToSimpleObject(deferred);

            var err = o as IBifoqlError;
            if (err != null) return ToSimpleObject(err);

            var index = o as IBifoqlIndexInternal;
            if (index != null) return ToSimpleObject(index);

            // Lookups can only be resolved through a query.
            // Making this undefined means that lookups will also be
            // excluded from dictionaries and arrays.
            var lookup = o as IBifoqlLookupInternal;
            if (lookup != null) return ToSimpleObject(new AsyncError("query must resolve to leaf nodes"));

            return Task.FromResult<object>(null);
        }

        private static async Task<object> ToSimpleObject(Func<Task<IBifoqlObject>> defaultValue)
        {
            var obj = await defaultValue();
            return await obj.ToSimpleObject();
        }

        private static async Task<object> ToSimpleObject(IBifoqlIndexInternal index)
        {
            // TODO - index lookup probably should return IBifoqlObject, not object.
            var resultWithZeroArgs = await index.Lookup(IndexArgumentList.CreateEmpty());
            if (resultWithZeroArgs is IBifoqlObject)
            {
                return await ((IBifoqlObject)resultWithZeroArgs).ToSimpleObject();
            }
            else
            {
                return resultWithZeroArgs;
            }
        }

        private static async Task<object> ToSimpleObject(IBifoqlMapInternal lookup)
        {
            var values = new Dictionary<string, object>();

            foreach (var pair in lookup)
            {
                var value = await pair.Value();

                if (value is IBifoqlUndefined || value is IBifoqlLookupBase) continue;

                values[pair.Key] = await ToSimpleObject(value);
            }

            return new DynamicDict(values);
        }

        private static async Task<object> ToSimpleObject(IBifoqlArrayInternal list)
        {
            var tasks = new List<Task<object>>();
            for (int i = 0; i < list.Count; i++)
            {
                var asyncObj = await list[i]();
                if (asyncObj is IBifoqlUndefined) continue;

                // If you try to resolve a lookup, 
                // we'll return null instead of hiding it so that we don't
                // change the size of the array.
                // This is different from maps, where we just hide lookup entries.
                // 
                // We could also make these situations an error.
                if (asyncObj is IBifoqlLookupBase)
                {
                    asyncObj = AsyncNull.Instance;
                }

                tasks.Add(asyncObj.ToSimpleObject());
            }

            return await Task.WhenAll(tasks.ToArray());
        }

        private static async Task<object> ConvertListEntryToSimpleObject(Func<Task<IBifoqlObject>> obj)
        {
            var asyncObj = await obj();
            return await asyncObj.ToSimpleObject();
        }

        private static async Task<object> ToSimpleObject(IBifoqlString str)
        {
            return await str.Value;
        }

        private static async Task<object> ToSimpleObject(IBifoqlNumber num)
        {
            var value = await num.Value;
            if ((int)value == value)
            {
                return (int)value;
            }
            else
            {
                return value;
            }
        }

        private static async Task<object> ToSimpleObject(IBifoqlBoolean boolean)
        {
            return await boolean.Value;
        }
        
        private static async Task<object> ToSimpleObject(IBifoqlDeferredQueryInternal deferred)
        {
            var obj = await deferred.EvaluateQuery("@");
            return obj;
        }
        
        private static Task<object> ToSimpleObject(IBifoqlError error)
        {
            return Task.FromResult<object>($"<error: {error.Message}>");
        }
    }
}
