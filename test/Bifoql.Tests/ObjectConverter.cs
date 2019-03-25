namespace Bifoql.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Types;
    using Newtonsoft.Json.Linq;

    public static class ObjectConverter
    {
        internal static IBifoqlObject ToAsyncObject(object o, BifoqlType schema=null)
        {
            if (o == null) return AsyncNull.Instance;

            if (o is IBifoqlObject) return (IBifoqlObject)o;

            if (o is DynamicDict) return ConvertDynamicDict((DynamicDict)o, schema);

            if (o is IDictionary) return ConvertDictionary(o, schema);

            if (o is JObject || o is JArray || o is JToken || o is JValue) return ConvertJToken(o, schema);

            if (o is bool) return new AsyncBoolean(Convert.ToBoolean(o), schema);

            if (o is int || o is uint || o is byte || o is sbyte || o is short || o is ushort || o is long || o is ulong || o is double || o is float)
                return new AsyncNumber(Convert.ToDouble(o), schema);

            if (o is string || o.GetType().IsValueType) return new AsyncString(o.ToString(), schema);

            if (o is IEnumerable) return ConvertList(o, schema);

            return PropertyAdapter.Create(o, o.GetType());
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
                    list.Add(() => Task.FromResult(ToAsyncObject(item, elementType)));
                }
            }

            return new AsyncArray(list, schema);
        }

        private static IBifoqlObject ConvertDictionary(object o, BifoqlType schema)
        {
            var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>();

            if (o is IDictionary<string, Task<IBifoqlObject>>)
            {
                foreach (var pair in ((IDictionary<string, Task<IBifoqlObject>>)o))
                {
                    dict[pair.Key] = () => pair.Value;
                }
                return new AsyncLookup(dict);
            }

            if (o is IDictionary<string, Func<IBifoqlObject>>)
            {
                foreach (var pair in ((IDictionary<string, Func<IBifoqlObject>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(pair.Value());
                }
                return new AsyncLookup(dict);
            }

            if (o is IDictionary<string, Func<object>>)
            {
                foreach (var pair in ((IDictionary<string, Func<object>>)o))
                {
                    dict[pair.Key] = () => Task.FromResult(ToAsyncObject(pair.Value()));
                }
                return new AsyncLookup(dict);
            }

            foreach (DictionaryEntry pair in (IDictionary)o)
            {
                var key = pair.Key.ToString();
                var valueType = schema?.GetKeyType(key);
                dict[key] = () => Task.FromResult(ToAsyncObject(pair.Value, valueType));
            }

            return new AsyncLookup(dict, schema);
        }

        private static IBifoqlObject ConvertDynamicDict(DynamicDict dict, BifoqlType schema)
        {
            var map = new Dictionary<string, Func<Task<IBifoqlObject>>>();

            foreach (var pair in dict)
            {
                var currSchema = schema?.GetKeyType(pair.Key);
                map[pair.Key] = () => Task.FromResult(ToAsyncObject(pair.Value, currSchema));
            }

            return new AsyncLookup(map, schema);
        }

        private static IBifoqlObject ConvertJToken(object j, BifoqlType schema)
        {
            var jobj = j as JObject;
            if (jobj != null)
            {
                var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>();
                foreach (var pair in jobj)
                {
                    var valueSchema = schema?.GetKeyType(pair.Key);
                    dict[pair.Key] = () => Task.FromResult(ToAsyncObject(pair.Value, valueSchema));
                }

                return new AsyncLookup(dict, schema);
            }

            var jarr = j as JArray;
            if (jarr != null)
            {
                var arr = new List<Func<Task<IBifoqlObject>>>();

                int i = 0;
                foreach (var el in jarr)
                {
                    var elementType = schema?.GetElementType(i++);
                    arr.Add(() => Task.FromResult(ToAsyncObject(el, elementType)));
                }

                return new AsyncArray(arr, schema);
            }

            var jval = j as JValue;
            if (jval != null)
            {
                var val = jval.Value;
                if (jval.Type == JTokenType.String)
                {
                    return new AsyncString((string)val);
                }
                else if (jval.Type == JTokenType.Float || jval.Type == JTokenType.Integer)
                {
                    return new AsyncNumber(Convert.ToDouble(val));
                }
                else if (jval.Type == JTokenType.Boolean)
                {
                    return new AsyncBoolean((bool)val);
                }
            }

            return new AsyncString(((JToken)j).Value<string>());
        }
    }
}