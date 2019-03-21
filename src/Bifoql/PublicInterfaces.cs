namespace Bifoql
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using Bifoql.Types;

    public interface IBifoqlSyncArray
    {
        IReadOnlyList<Func<object>> Entries { get; }
    }

    public interface IBifoqlSyncMap
    {
        IEnumerable<string> Keys { get; }
        Func<object> this[string key] { get; }
    }

    public interface IBifoqlAsyncArray
    {
        int Count { get; }
        Task<Func<object>> this[int i] { get; }
    }

    public interface IBifoqlAsyncMap
    {
        IEnumerable<string> Keys { get; }
        Task<Func<object>> this[string key] { get; }
    }
}