using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncBoolean : AsyncObjectBase, IAsyncBoolean
    {
        public Task<bool> Value { get; }
        private BifoqlType _schema;

        public AsyncBoolean(bool value, BifoqlType schema=null)
        {
            Value = Task.FromResult(value);
            _schema = schema ?? BifoqlType.Boolean;
        }

        public async Task<bool> IsEqualTo(IAsyncObject other)
        {
            if (this == other) return true;
            
            var o = other as IAsyncBoolean;
            if (o == null) return false;

            return (await o.Value) == await Value;
        }
        
        public Task<BifoqlType> GetSchema() => Task.FromResult<BifoqlType>(_schema);
    }
}