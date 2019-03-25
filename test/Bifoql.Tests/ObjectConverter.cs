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

            if (o is JObject || o is JArray || o is JToken || o is JValue) return ConvertJToken(o, schema);

            return Bifoql.Extensions.BifoqlExtensions.ToBifoqlObject(o, schema);
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

                return new AsyncMap(dict, schema);
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