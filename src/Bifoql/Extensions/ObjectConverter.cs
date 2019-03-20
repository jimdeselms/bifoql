using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bifoql.Adapters;
using Bifoql.Types;

namespace Bifoql.Extensions
{
    public static class ObjectConverter
    {
        public static Task<object> ToSimpleObject(this IAsyncObject o, BifoqlType expectedSchema=null)
        {
            var lookup = o as IAsyncMap;
            if (lookup != null) return ToSimpleObject(lookup, expectedSchema);

            var arr = o as IAsyncArray;
            if (arr != null) return ToSimpleObject(arr, expectedSchema);

            var str = o as IAsyncString;
            if (str != null) return ToSimpleObject(str, expectedSchema);

            var num = o as IAsyncNumber;
            if (num != null) return ToSimpleObject(num, expectedSchema);

            var boolean = o as IAsyncBoolean;
            if (boolean != null) return ToSimpleObject(boolean, expectedSchema);

            var deferred = o as IAsyncDeferredQuery;
            if (deferred != null) return ToSimpleObject(deferred, expectedSchema);

            var err = o as IAsyncError;
            if (err != null) return ToSimpleObject(err, expectedSchema);

            return Task.FromResult<object>(null);
        }

        private static async Task<object> ToSimpleObject(IAsyncMap lookup, BifoqlType expectedSchema)
        {
            await AssertSchema(lookup, expectedSchema);

            var values = new Dictionary<string, object>();

            foreach (var pair in lookup)
            {
                var value = await pair.Value();

                if (value != null)
                {
                    values[pair.Key] = await ToSimpleObject(value);
                }
            }

            return new DynamicDict(values);
        }

        private static async Task<object> ToSimpleObject(IAsyncArray list, BifoqlType expectedSchema)
        {
            var tasks = new Task<object>[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var elementType = expectedSchema?.GetElementType(i);
                tasks[i] = ConvertListEntryToSimpleObject(list[i], elementType);
            }

            return await Task.WhenAll(tasks);
        }

        private static async Task<object> ConvertListEntryToSimpleObject(Func<Task<IAsyncObject>> obj, BifoqlType expectedType)
        {
            var asyncObj = await obj();
            return await asyncObj.ToSimpleObject(expectedType);
        }

        private static async Task<object> ToSimpleObject(IAsyncString str, BifoqlType expectedSchema)
        {
            AssertSchema(BifoqlType.String, expectedSchema);
            return await str.Value;
        }

        private static async Task<object> ToSimpleObject(IAsyncNumber num, BifoqlType expectedSchema)
        {
            AssertSchema(BifoqlType.Number, expectedSchema);
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

        private static async Task<object> ToSimpleObject(IAsyncBoolean boolean, BifoqlType expectedSchema)
        {
            AssertSchema(BifoqlType.Boolean, expectedSchema);
            return await boolean.Value;
        }
        
        private static async Task<object> ToSimpleObject(IAsyncDeferredQuery deferred, BifoqlType expectedSchema)
        {
            var obj = await deferred.EvaluateQuery("@");
            await AssertSchema(obj, expectedSchema);
            return await obj.ToSimpleObject(expectedSchema);
        }
        
        private static Task<object> ToSimpleObject(IAsyncError error, BifoqlType expectedSchema)
        {
            AssertSchema(BifoqlType.Error, expectedSchema);
            return Task.FromResult<object>($"<error: {error.Message}>");
        }

        private static async Task AssertSchema(IAsyncObject asyncObject, BifoqlType expectedSchema)
        {
            // No expected schema? Then no assertion.
            if (expectedSchema == null) return;

            var actualSchema = await asyncObject.GetSchema();
            if (!TypeCompatibilityChecker.IsCompatible(actualSchema, expectedSchema))
            {
                throw new Exception($"Schema mismatch. Expected {expectedSchema.ToString()} Actual {actualSchema.ToString()}");
            }
        }

        private static async Task AssertSchema(IAsyncMap asyncMap, BifoqlType expectedSchema)
        {
            // No expected schema? Then no assertion.
            if (expectedSchema == null) return;

            bool fail = false;

            var mapType = expectedSchema as MapType;
            if (mapType != null)
            {
                foreach (var pair in mapType.Properties)
                {
                    // optional types are okay if they're not defined
                    if (pair.Value is OptionalType && !asyncMap.ContainsKey(pair.Key)) continue;

                    if (!asyncMap.ContainsKey(pair.Key))
                    {
                        fail = true;
                        break;
                    }
                    var value = await asyncMap[pair.Key]();
                    var valueType = await value.GetSchema();

                    if (!TypeCompatibilityChecker.IsCompatible(valueType, pair.Value))
                    {
                        fail = true;
                        break;
                    }
                }
            }
            else
            {
                fail = true;
            }

            if (fail)
            {
                var actualSchema = await asyncMap.GetSchema();
                throw new Exception($"Schema mismatch. Expected {expectedSchema.ToString()} Actual {actualSchema.ToString()}");
            }
        }

        private static async Task AssertSchema(IAsyncArray asyncArray, BifoqlType expectedSchema)
        {
            // No expected schema? Then no assertion.
            if (expectedSchema == null) return;

            var actualSchema = await asyncArray.GetSchema();
            if (!TypeCompatibilityChecker.IsCompatible(actualSchema, expectedSchema))
            {
                throw new Exception($"Schema mismatch. Expected {expectedSchema.ToString()} Actual {actualSchema.ToString()}");
            }
        }


        private static void AssertSchema(BifoqlType actualSchema, BifoqlType expectedSchema)
        {
            if (expectedSchema == null) return;

            if (!TypeCompatibilityChecker.IsCompatible(actualSchema, expectedSchema))
            {
                throw new Exception($"Schema mismatch. Expected {expectedSchema.ToString()} Actual {actualSchema.ToString()}");
            }
        }
    }
}
