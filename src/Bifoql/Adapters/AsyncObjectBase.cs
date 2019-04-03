namespace Bifoql.Adapters
{
    using System;
    using System.Threading.Tasks;

    internal class AsyncObjectBase
    {
    }

    internal class AsyncObjectWithDefaultValueBase : AsyncObjectBase, IBifoqlHasDefaultValue
    {
        protected AsyncObjectWithDefaultValueBase(Func<Task<IBifoqlObject>> defaultValue)
        {
            _defaultValue = defaultValue;
        }
        private readonly Func<Task<IBifoqlObject>> _defaultValue;
        public Func<Task<IBifoqlObject>> GetDefaultValue()
        {
            return _defaultValue;
        }
    }
}