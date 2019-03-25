namespace Bifoql.Extensions
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Types;

    public static class BifoqlExtensions
    {
        internal static IBifoqlObject ToBifoqlObject(this object o, BifoqlType schema=null)
        {
            if (o == null) return AsyncNull.Instance;

            if (o is IBifoqlArray) return ConvertAsyncArray((IBifoqlArray)o);
            if (o is IBifoqlArraySync) return ConvertSyncArray((IBifoqlArraySync)o);

            if (o is IBifoqlMap) return ConvertAsyncMap((IBifoqlMap)o);
            if (o is IBifoqlMapSync) return ConvertSyncMap((IBifoqlMapSync)o);

            if (o is IBifoqlLookup) return new AsyncPureLookup(((IBifoqlLookup)o));
            if (o is IBifoqlLookupSync) return new SyncPureLookup((IBifoqlLookupSync)o);

            if (o is IBifoqlIndex) return ConvertAsyncIndex((IBifoqlIndex)o);
            if (o is IBifoqlIndexSync) return ConvertSyncIndex((IBifoqlIndexSync)o);

            if (o is IBifoqlObject) return (IBifoqlObject)o;

            if (o is DynamicDict) return ConvertDynamicDict((DynamicDict)o, schema);

            if (o is IDictionary) return ConvertDictionary(o, schema, toLookup: false);

            if (o is bool) return new AsyncBoolean(Convert.ToBoolean(o), schema);

            if (o is int || o is uint || o is byte || o is sbyte || o is short || o is ushort || o is long || o is ulong || o is double || o is float)
                return new AsyncNumber(Convert.ToDouble(o), schema);

            if (o is string || o.GetType().IsValueType) return new AsyncString(o.ToString(), schema);

            if (o is IEnumerable) return ConvertList(o, schema);

            return PropertyAdapter.Create(o, o.GetType());
        }

        internal static IBifoqlObject ToBifoqlMap(this IDictionary dictionary, BifoqlType schema=null)
        {
            return ConvertDictionary(dictionary, schema, toLookup: false);
        }

        private static IBifoqlObject ConvertAsyncArray(IBifoqlArray a)
        {
            var items = a.Items.Select(i => (Func<Task<IBifoqlObject>>)(async () => (await i()).ToBifoqlObject()));
            return new AsyncArray(items.ToList());
        }

        private static IBifoqlObject ConvertAsyncMap(IBifoqlMap m)
        {
            var map = m.Items.ToDictionary(
                pair => pair.Key,
                pair => (Func<Task<IBifoqlObject>>)(async () => (await pair.Value()).ToBifoqlObject()));

            return new AsyncMap(map);
        }

        private static IBifoqlObject ConvertSyncArray(IBifoqlArraySync a)
        {
            var items = a.Items.Select(i => (Func<Task<IBifoqlObject>>)(() => Task.FromResult(i().ToBifoqlObject())));
            return new AsyncArray(items.ToList());
        }

        private static IBifoqlObject ConvertSyncMap(IBifoqlMapSync m)
        {
            var map = m.Items.ToDictionary(
                pair => pair.Key,
                pair => (Func<Task<IBifoqlObject>>)(() => Task.FromResult(pair.Value().ToBifoqlObject())));

            return new AsyncMap(map);
        }

        private static IBifoqlObject ConvertAsyncIndex(IBifoqlIndex i)
        {
            return new AsyncIndex(list => i.Lookup(list));
        }

        private static IBifoqlObject ConvertSyncIndex(IBifoqlIndexSync i)
        {
            return new AsyncIndex(list => Task.FromResult(i.Lookup(list)));
        }

        private static IBifoqlObject ConvertList(object o, BifoqlType schema)
        {
            var list = new List<Func<Task<IBifoqlObject>>>();

            int i = 0;

            if (o is IList<Func<IBifoqlObject>>)
            {
                foreach (var item in ((IList<Func<IBifoqlObject>>)o))
                {
                    list.Add(() => Task.FromResult(item()));
                }
            }
            else
            {
                foreach (object item in (IEnumerable)o)
                {
                    var elementType = schema?.GetElementType(i++);
                    list.Add(() => Task.FromResult(item.ToBifoqlObject(elementType)));
                }
            }

            return new AsyncArray(list, schema);
        }

        private static IBifoqlObject ConvertDictionary(object o, BifoqlType schema, bool toLookup)
        {
            var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>();

            if (o is IDictionary<string, Task<IBifoqlObject>>)
            {
                foreach (var pair in ((IDictionary<string, Task<IBifoqlObject>>)o))
                {
                    dict[pair.Key] = () => pair.Value;
                }
                return toLookup ? (IBifoqlObject)new AsyncLookup(dict) : new AsyncMap(dict);
            }

            if (o is IDictionary<string, Func<IBifoqlObject>>)
            {
                foreach (var pair in ((IDictionary<string, Func<IBifoqlObject>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value());
                }
                return toLookup ? (IBifoqlObject)new AsyncLookup(dict) : new AsyncMap(dict);
            }

            if (o is IDictionary<string, Func<object>>)
            {
                foreach (var pair in ((IDictionary<string, Func<object>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value().ToBifoqlObject());
                }
                return toLookup ? (IBifoqlObject)new AsyncLookup(dict) : new AsyncMap(dict);
            }

            foreach (DictionaryEntry pair in (IDictionary)o)
            {
                var key = pair.Key.ToString();
                var valueType = schema?.GetKeyType(key);
                dict[key] = () => Task.FromResult(pair.Value.ToBifoqlObject(valueType));
            }

            return toLookup ? (IBifoqlObject)new AsyncLookup(dict) : new AsyncMap(dict);
        }

        private static IBifoqlObject ConvertDynamicDict(DynamicDict dict, BifoqlType schema)
        {
            var map = new Dictionary<string, Func<Task<IBifoqlObject>>>();

            foreach (var pair in dict)
            {
                var currSchema = schema?.GetKeyType(pair.Key);
                map[pair.Key] = () => Task.FromResult(pair.Value.ToBifoqlObject(currSchema));
            }

            return new AsyncMap(map, schema);
        }
    }
}