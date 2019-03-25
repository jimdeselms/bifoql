using System;
using System.IO;
using Bifoql;
using Bifoql.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bifoql.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            object input = null;
            string query = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-q")
                {
                    query = args[i + 1];
                }
                else if (args[i] == "-i")
                {
                    // We'll read the input in as a literal.
                    var json = File.ReadAllText(args[i+1]);
                    var jobj = JsonConvert.DeserializeObject<object>(json);
                    input = ObjectConverter.ToAsyncObject(jobj);
                }
            }

            if (query == null)
            {
                query = "";
                Console.WriteLine("Enter your query. Hit ctrl-z when done.");
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == null || string.IsNullOrEmpty(line)) break;

                    query += line + "\n";
                }
            }

            var compiledQuery = Query.Compile(query);
            var result = compiledQuery.Run(input).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}
