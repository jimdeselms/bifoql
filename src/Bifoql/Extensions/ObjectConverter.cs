using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bifoql.Adapters;
using Bifoql.Types;

namespace Bifoql.Extensions
{
    public static class ObjectConverter
    {
        internal static Task<object> ToSimpleObject(this IBifoqlObject o)
        {
            var lookup = o as IBifoqlMapInternal;
            if (lookup != null) return ToSimpleObject(lookup);

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

            return Task.FromResult<object>(null);
        }

        private static async Task<object> ToSimpleObject(IBifoqlMapInternal lookup)
        {
            var values = new Dictionary<string, object>();

            foreach (var pair in lookup)
            {
                var value = await pair.Value();

                if (value == null || value is IBifoqlUndefined) continue;

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
