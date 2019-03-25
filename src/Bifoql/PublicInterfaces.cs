namespace Bifoql
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using Bifoql.Types;

    public interface IBifoqlArraySync
    {
        IReadOnlyList<Func<object>> Items { get; }
    }

    public interface IBifoqlMapSync
    {
        IReadOnlyDictionary<string, Func<object>> Items { get; }
    }

    public interface IBifoqlLookupBase
    {
    }

    public interface IBifoqlLookupSync : IBifoqlLookupBase
    {
        bool TryGetValue(string key, out Func<object> result);
    }

    public interface IBifoqlArray
    {
        IReadOnlyList<Func<Task<object>>> Items { get; }
    }

    public interface IBifoqlMap
    {
        IReadOnlyDictionary<string, Func<Task<object>>> Items { get; }
    }

    public interface IBifoqlLookup : IBifoqlLookupBase
    {
        bool TryGetValue(string key, out Func<Task<object>> result);
    }

    public interface IBifoqlIndexSync
    {
        object Lookup(IIndexArgumentListSync args);
    }

    /// <summary>
    /// An object that allows you to pass arguments and get a result back.
    /// </summary>
    public interface IBifoqlIndex
    {
        Task<object> Lookup(IIndexArgumentList args);
    }

    /// <summary>
    /// Represents a call to another Bifoql service; a Bifoql query
    /// can actually be serviced by lots of calls to other Bifoql services.
    /// </summary>
    public interface IBifoqlDeferredQuery
    {
        Task<object> EvaluateQuery(string query);
    }
}