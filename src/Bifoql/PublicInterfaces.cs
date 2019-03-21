namespace Bifoql
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using Bifoql.Types;

    internal interface ISyncBifoqlArray
    {
        IReadOnlyList<Func<object>> Items { get; }
    }

    internal interface ISyncBifoqlMap
    {
        IReadOnlyDictionary<string, Func<object>> Items { get; }
    }

    internal interface IAsyncBifoqlArray
    {
        IReadOnlyList<Func<Task<object>>> Items { get; }
    }

    internal interface IAsyncBifoqlMap
    {
        IReadOnlyDictionary<string, Func<Task<object>>> Items { get; }
    }

    public interface ISyncBifoqlIndex
    {
        object Lookup(ISyncIndexArgumentList args);
    }

    public interface IAsyncBifoqlIndex
    {
        Task<object> Lookup(IAsyncIndexArgumentList args);
    }
}