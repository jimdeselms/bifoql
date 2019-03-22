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

    public interface IBifoqlArray
    {
        IReadOnlyList<Func<Task<object>>> Items { get; }
    }

    public interface IBifoqlMap
    {
        IReadOnlyDictionary<string, Func<Task<object>>> Items { get; }
    }

    public interface IBifoqlIndexSync
    {
        object Lookup(IIndexArgumentListSync args);
    }

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