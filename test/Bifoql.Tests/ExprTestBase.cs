using System;
using Xunit;
using Bifoql;
using Bifoql.Extensions;
using Bifoql.Tests.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Bifoql.Tests
{
    public class ExprTestBase
    {
        protected static async Task RunTestAsync(object expected, string query, object input=null, IReadOnlyDictionary<string, object> arguments=null, IReadOnlyDictionary<string, CustomFunction> customFunctions=null)
        {
            if (input is JToken)
            {
                input = Helpers.ObjectConverter.ToBifoqlObject(input);
            }
            
            IBifoqlObject inputObj = input?.ToBifoqlObject();

            var result = await Query(inputObj, query, arguments, customFunctions);

            var actualJson = JsonConvert.SerializeObject(result);

            var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(expected)));

            Assert.Equal(expectedJson, actualJson);
        }

        protected static void RunTest(object expected, string query, object input=null, IReadOnlyDictionary<string, object> arguments=null, IReadOnlyDictionary<string, CustomFunction> customFunctions=null)
        {
            if (input is JToken)
            {
                input = Helpers.ObjectConverter.ToBifoqlObject(input);
            }
            
            IBifoqlObject inputObj = input?.ToBifoqlObject();

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
            var variables = arguments?.Keys?.ToArray() ?? new string[0];
            var queryObj = Bifoql.Query.Compile(query, variables, customFunctions);
            var asyncObj = o.ToBifoqlObject();

            return await queryObj.Run(asyncObj, arguments);
        }
    }
}
