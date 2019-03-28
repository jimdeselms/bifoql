using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts([FromBody]BifoqlRequest bifoql)
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = bifoql?.Query ?? "@",
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
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
