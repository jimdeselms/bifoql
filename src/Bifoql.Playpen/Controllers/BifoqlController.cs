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
        private static string[] Summaries = new[]
        {
            "Freaking cold", "Bracing", "Nippy", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Holy crap it's hot"
        };

        public class BifoqlRequest
        {
            public string Query;
            public string Input;
        }

        private static readonly TestModel TEST_MODEL = new TestModel();

        [HttpPost("[action]")]
        public async Task<object> WeatherForecasts([FromBody]BifoqlRequest bifoql)
        {
            object input = string.IsNullOrWhiteSpace(bifoql.Input)
                ? (object)TEST_MODEL
                : ObjectConverter.ToBifoqlObject(JsonConvert.DeserializeObject<object>(bifoql.Input));

            var query = Bifoql.Query.Compile(bifoql.Query ?? "@");
            return JsonConvert.SerializeObject(await query.Run(input), Formatting.Indented);
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get
                {
                    return 32 + (int)(TemperatureC / 0.5556);
                }
            }
        }
    }
}
