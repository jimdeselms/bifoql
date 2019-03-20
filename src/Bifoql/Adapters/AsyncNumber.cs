using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class AsyncNumber : AsyncObjectBase, IAsyncNumber
    {
        public Task<double> Value { get; }
        private BifoqlType _schema;

        public AsyncNumber(double value, BifoqlType schema=null)
        {
            Value = Task.FromResult(value);
            _schema = schema ?? BifoqlType.Number;
        }

        public async Task<bool> IsEqualTo(IAsyncObject other)
        {
            if (this == other) return true;
            
            var o = other as IAsyncNumber;
            if (o == null) return false;

            return (await o.Value) == await Value;
        }
        public Task<BifoqlType> GetSchema() => Task.FromResult<BifoqlType>(_schema);
    }
}