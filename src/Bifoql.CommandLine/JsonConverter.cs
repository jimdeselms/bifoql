namespace Bifoql.CommandLine
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
        internal static IBifoqlObject ToAsyncObject(object o)
        {
            if (o == null) return AsyncNull.Instance;

            if (o is JObject || o is JArray || o is JToken || o is JValue) return ConvertJToken(o);

            return Bifoql.Extensions.BifoqlExtensions.ToBifoqlObject(o);
        }

        private static IBifoqlObject ConvertJToken(object j)
        {
            var jobj = j as JObject;
            if (jobj != null)
            {
                var dict = new Dictionary<string, Func<Task<IBifoqlObject>>>();
                foreach (var pair in jobj)
                {
                    dict[pair.Key] = () => Task.FromResult(ToAsyncObject(pair.Value));
                }

                return new AsyncMap(dict, null);
            }

            var jarr = j as JArray;
            if (jarr != null)
            {
                var arr = new List<Func<Task<IBifoqlObject>>>();

                foreach (var el in jarr)
                {
                    arr.Add(() => Task.FromResult(ToAsyncObject(el)));
                }

                return new AsyncArray(arr);
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