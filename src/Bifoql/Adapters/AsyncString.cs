using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncString : AsyncObjectBase, IBifoqlString
    {
        public Task<string> Value { get; }
        private readonly BifoqlType _schema;

        public AsyncString(string value, BifoqlType schema=null)
        {
            Value = Task.FromResult(value);
            _schema = schema ?? BifoqlType.String;
        }

        public async Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) return true;
            
            var o = other as IBifoqlString;
            if (o == null) return false;

            return (await o.Value) == await Value;
        }
    }
}