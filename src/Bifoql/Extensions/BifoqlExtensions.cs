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
        internal static IBifoqlObject ToBifoqlObject(this object o)
        {
            if (o is IBifoqlObject) return (IBifoqlObject)o;

            if (o == null) return AsyncNull.Instance;

            var defaultValue = GetDefaultValue(o);

            if (o is IBifoqlArray) return ConvertAsyncArray((IBifoqlArray)o);
            if (o is IBifoqlArraySync) return ConvertSyncArray((IBifoqlArraySync)o);

            if (o is IBifoqlMap) return ConvertAsyncMap((IBifoqlMap)o, defaultValue);
            if (o is IBifoqlMapSync) return ConvertSyncMap((IBifoqlMapSync)o, defaultValue);

            if (o is IBifoqlLookup) return new AsyncPureLookup(((IBifoqlLookup)o), defaultValue);
            if (o is IBifoqlLookupSync) return new SyncPureLookup((IBifoqlLookupSync)o, defaultValue);

            if (o is IBifoqlIndex) return ConvertAsyncIndex((IBifoqlIndex)o, defaultValue);
            if (o is IBifoqlIndexSync) return ConvertSyncIndex((IBifoqlIndexSync)o, defaultValue);

            if (o is DynamicDict) return ConvertDynamicDict((DynamicDict)o);

            if (o is IDictionary) return ConvertDictionary(o, toLookup: false);

            if (o is bool) return new AsyncBoolean(Convert.ToBoolean(o));

            if (o is int || o is uint || o is byte || o is sbyte || o is short || o is ushort || o is long || o is ulong || o is double || o is float)
                return new AsyncNumber(Convert.ToDouble(o));

            if (o is string || o.GetType().IsValueType) return new AsyncString(o.ToString());

            if (o is IEnumerable) return ConvertList(o);

            return PropertyAdapter.Create(o, o.GetType(), defaultValue);
        }

        internal static Func<Task<IBifoqlObject>> GetDefaultValue(object o)
        {
            var defaultValue = o as IDefaultValue;
            if (defaultValue != null)
            {
                return async () => {
                    var v = await defaultValue.GetDefaultValue();
                    return v.ToBifoqlObject();
                };
            }

            var defaultValueSync = o as IDefaultValueSync;
            if (defaultValueSync != null)
            {
                return () => Task.FromResult(defaultValueSync.GetDefaultValue().ToBifoqlObject());
            }

            return null;
        }

        internal static IBifoqlObject ToBifoqlMap(this IDictionary dictionary)
        {
            return ConvertDictionary(dictionary, toLookup: false);
        }

        private static IBifoqlObject ConvertAsyncArray(IBifoqlArray a)
        {
            var items = a.Items.Select(i => (Func<Task<IBifoqlObject>>)(async () => (await i()).ToBifoqlObject()));
            return new AsyncArray(items.ToList());
        }

        private static IBifoqlObject ConvertAsyncMap(IBifoqlMap m, Func<Task<IBifoqlObject>> defaultValue)
        {
            var map = m.Items.ToDictionary(
                pair => pair.Key,
                pair => (Func<Task<IBifoqlObject>>)(async () => (await pair.Value()).ToBifoqlObject()));

            return new AsyncMap(map, defaultValue);
        }

        private static IBifoqlObject ConvertSyncArray(IBifoqlArraySync a)
        {
            var items = a.Items.Select(i => (Func<Task<IBifoqlObject>>)(() => Task.FromResult(i().ToBifoqlObject())));
            return new AsyncArray(items.ToList());
        }

        private static IBifoqlObject ConvertSyncMap(IBifoqlMapSync m, Func<Task<IBifoqlObject>> defaultValue)
        {
            var map = m.Items.ToDictionary(
                pair => pair.Key,
                pair => (Func<Task<IBifoqlObject>>)(() => Task.FromResult(pair.Value().ToBifoqlObject())));

            return new AsyncMap(map, defaultValue);
        }

        private static IBifoqlObject ConvertAsyncIndex(IBifoqlIndex i, Func<Task<IBifoqlObject>> defaultValue)
        {
            return new AsyncIndex(list => i.Lookup(list), defaultValue);
        }

        private static IBifoqlObject ConvertSyncIndex(IBifoqlIndexSync i, Func<Task<IBifoqlObject>> defaultValue)
        {
            return new AsyncIndex(list => Task.FromResult(i.Lookup(list)), defaultValue);
        }

        private static IBifoqlObject ConvertList(object o)
        {
            var list = new List<Func<Task<IBifoqlObject>>>();

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
                    list.Add(() => Task.FromResult(item.ToBifoqlObject()));
                }
            }

            return new AsyncArray(list);
        }

        private static IBifoqlObject ConvertDictionary(object o, bool toLookup)
        {
            var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>();

            if (o is IDictionary<string, Task<IBifoqlObject>>)
            {
                foreach (var pair in ((IDictionary<string, Task<IBifoqlObject>>)o))
                {
                    dict[pair.Key] = () => pair.Value;
                }
                return toLookup ? (IBifoqlObject)new AsyncLookup(dict, null) : new AsyncMap(dict, null);
            }

            if (o is IDictionary<string, Func<IBifoqlObject>>)
            {
                foreach (var pair in ((IDictionary<string, Func<IBifoqlObject>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value());
                }
                return toLookup ? (IBifoqlObject)new AsyncLookup(dict, null) : new AsyncMap(dict, null);
            }

            if (o is IDictionary<string, Func<object>>)
            {
                foreach (var pair in ((IDictionary<string, Func<object>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value().ToBifoqlObject());
                }
                return toLookup ? (IBifoqlObject)new AsyncLookup(dict, null) : new AsyncMap(dict, null);
            }

            foreach (DictionaryEntry pair in (IDictionary)o)
            {
                var key = pair.Key.ToString();
                dict[key] = () => Task.FromResult(pair.Value.ToBifoqlObject());
            }

            return toLookup ? (IBifoqlObject)new AsyncLookup(dict, null) : new AsyncMap(dict, null);
        }

        private static IBifoqlObject ConvertDynamicDict(DynamicDict dict)
        {
            var map = new Dictionary<string, Func<Task<IBifoqlObject>>>();

            foreach (var pair in dict)
            {
                map[pair.Key] = () => Task.FromResult(pair.Value.ToBifoqlObject());
            }

            return new AsyncMap(map, null);
        }
    }
}