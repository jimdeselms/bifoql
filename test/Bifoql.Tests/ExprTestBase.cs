using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bifoql.Tests
{
    public class ExprTestBase
    {
        protected static void RunTest(object expected, string query, object input=null, IReadOnlyDictionary<string, object> arguments=null, IReadOnlyDictionary<string, CustomFunction> customFunctions=null)
        {
            IBifoqlObject inputObj = input as IBifoqlObject;

            if (inputObj == null)
            {
                var asyncObj = input?.ToBifoqlObject();
                var inputJson = JsonConvert.SerializeObject(input);
                var jobject = JsonConvert.DeserializeObject<object>(inputJson);
                var originalJson = JsonConvert.SerializeObject(jobject);
                inputObj = ObjectConverter.ToAsyncObject(jobject);
            }

            var result = Query(inputObj, query, arguments, customFunctions).Result;

            var actualJson = JsonConvert.SerializeObject(result);

            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        protected static object ParseObj(string json)
        {
            return JsonConvert.DeserializeObject<object>(json);
        }

        protected static async Task<object> Query(object o, string query, IReadOnlyDictionary<string, object> arguments, IReadOnlyDictionary<string, CustomFunction> customFunctions)
        {
            var queryObj = Bifoql.Query.Compile(query, customFunctions);
            var asyncObj = o.ToBifoqlObject();

            return await queryObj.Run(asyncObj, arguments);
        }
    }
}
