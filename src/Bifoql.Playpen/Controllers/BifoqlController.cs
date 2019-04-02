using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bifoql;
using Bifoql.Playpen.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Bifoql.Playpen.Model;

namespace Bifoql.Playpen.Controllers
{
    [Route("api/[controller]")]
    public class BifoqlController : Controller
    {
        public class BifoqlRequest
        {
            public string Query;
            public string Input;
            public Dictionary<string, object> Arguments;
        }

        private static readonly TestModel TEST_MODEL = new TestModel();

        [HttpPost("")]
        public async Task<object> Query([FromBody]BifoqlRequest bifoql)
        {
            object input = string.IsNullOrWhiteSpace(bifoql.Input)
                ? (object)TEST_MODEL
                : ObjectConverter.ToBifoqlObject(JsonConvert.DeserializeObject<object>(bifoql.Input));

            var query = Bifoql.Query.Compile(bifoql.Query ?? "@");

            return JsonConvert.SerializeObject(await query.Run(input, bifoql.Arguments), Formatting.Indented);
        }
    }
}
