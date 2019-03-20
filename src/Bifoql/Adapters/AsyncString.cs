using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncString : AsyncObjectBase, IAsyncString
    {
        public Task<string> Value { get; }
        private readonly BifoqlType _schema;

        public AsyncString(string value, BifoqlType schema=null)
        {
            Value = Task.FromResult(value);
            _schema = schema ?? BifoqlType.String;
        }

        public async Task<bool> IsEqualTo(IAsyncObject other)
        {
            if (this == other) return true;
            
            var o = other as IAsyncString;
            if (o == null) return false;

            return (await o.Value) == await Value;
        }

        public Task<BifoqlType> GetSchema() => Task.FromResult<BifoqlType>(_schema);
    }
}