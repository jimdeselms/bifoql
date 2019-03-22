using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bifoql.Extensions;
using Bifoql.Adapters;
using Bifoql.Expressions;
using Bifoql.Types;

namespace Bifoql
{
    public class Query
    {
        internal Expr Expr { get; }

        internal Query(Expr expr)
        {
            Expr = expr;
        }

        public static Query Compile(string query, IReadOnlyDictionary<string, CustomFunction> customFunctions=null)
        {
            var parser = new QueryParser(customFunctions);
            var expr = parser.Parse(query);
            var simplified = expr.Simplify(new Dictionary<string, IBifoqlObject>());

            return new Query(simplified);
        }

        public async Task<object> Run(object queryTarget, IReadOnlyDictionary<string, object> arguments=null, bool validateSchema=false)
        {
            var obj = await GetResult(queryTarget, arguments);

            return await obj.ToSimpleObject();
        }

        private async Task<IBifoqlObject> GetResult(object queryTarget, IReadOnlyDictionary<string, object> arguments)
        {
            var asyncQueryTarget = queryTarget.ToBifoqlObject();
            var variables = new Dictionary<string, IBifoqlObject>
            {
                [""] = asyncQueryTarget
            };

            if (arguments != null)
            {
                foreach (var pair in arguments)
                {
                    variables[pair.Key.TrimStart('$')] = pair.Value.ToBifoqlObject();
                }
            }

            var context = new QueryContext(asyncQueryTarget, variables);
            return await Expr.Apply(context);
        }
    }
}
