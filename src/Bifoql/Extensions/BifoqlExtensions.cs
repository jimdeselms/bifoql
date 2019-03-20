namespace Bifoql.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Types;

    public static class BifoqlExtensions
    {
        public static IAsyncObject ToAsyncObject(this object o, BifoqlType schema=null)
        {
            if (o == null) return AsyncNull.Instance;

            if (o is IAsyncObject) return (IAsyncObject)o;

            if (o is DynamicDict) return ConvertDynamicDict((DynamicDict)o, schema);

            if (o is IDictionary) return ConvertDictionary(o, schema);

            if (o is bool) return new AsyncBoolean(Convert.ToBoolean(o), schema);

            if (o is int || o is uint || o is byte || o is sbyte || o is short || o is ushort || o is long || o is ulong || o is double || o is float)
                return new AsyncNumber(Convert.ToDouble(o), schema);

            if (o is string || o.GetType().IsValueType) return new AsyncString(o.ToString(), schema);

            if (o is IEnumerable) return ConvertList(o, schema);

            return PropertyAdapter.Create(o, o.GetType());
        }

        private static IAsyncObject ConvertList(object o, BifoqlType schema)
        {
            var list = new List<Func<Task<IAsyncObject>>>();

            int i = 0;

            if (o is IList<Func<IAsyncObject>>)
            {
                foreach (var item in ((IList<Func<IAsyncObject>>)o))
                {
                    list.Add(() => Task.FromResult(item()));
                }
            }
            else
            {
                foreach (object item in (IEnumerable)o)
                {
                    var elementType = schema?.GetElementType(i++);
                    list.Add(() => Task.FromResult(item.ToAsyncObject(elementType)));
                }
            }

            return new AsyncArray(list, schema);
        }

        private static IAsyncObject ConvertDictionary(object o, BifoqlType schema)
        {
            var dict = new Dictionary<string, Func<Task<IAsyncObject>>>();

            if (o is IDictionary<string, Task<IAsyncObject>>)
            {
                foreach (var pair in ((IDictionary<string, Task<IAsyncObject>>)o))
                {
                    dict[pair.Key] = () => pair.Value;
                }
                return new AsyncMap(dict);
            }

            if (o is IDictionary<string, Func<IAsyncObject>>)
            {
                foreach (var pair in ((IDictionary<string, Func<IAsyncObject>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value());
                }
                return new AsyncMap(dict);
            }

            if (o is IDictionary<string, Func<object>>)
            {
                foreach (var pair in ((IDictionary<string, Func<object>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value().ToAsyncObject());
                }
                return new AsyncMap(dict);
            }

            foreach (DictionaryEntry pair in (IDictionary)o)
            {
                var key = pair.Key.ToString();
                var valueType = schema?.GetKeyType(key);
                dict[key] = () => Task.FromResult(pair.Value.ToAsyncObject(valueType));
            }

            return new AsyncMap(dict, schema);
        }

        private static IAsyncObject ConvertDynamicDict(DynamicDict dict, BifoqlType schema)
        {
            var map = new Dictionary<string, Func<Task<IAsyncObject>>>();

            foreach (var pair in dict)
            {
                var currSchema = schema?.GetKeyType(pair.Key);
                map[pair.Key] = () => Task.FromResult(pair.Value.ToAsyncObject(currSchema));
            }

            return new AsyncMap(map, schema);
        }
    }
}